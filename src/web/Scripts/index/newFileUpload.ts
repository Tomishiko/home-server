export type UploaderConfig = {
    chunkSize?: number;        // bytes per chunk (default 512 KB)
    concurrency?: number;      // parallel chunk uploads (default 4)
    maxRetries?: number;       // per-chunk retries (default 3)
    backoffBaseMs?: number;    // base for exponential backoff (default 500ms)
    uploadUrl: string;         // chunk upload endpoint
    handshakeUrl: string;      // handshake endpoint
    timeoutMs?: number;        // optional per-chunk timeout
    resume?: boolean;          // whether to ask server which chunks exist
};

type HandshakeResponse = {
    uid: string;
    uploadedParts?: number[]; // optional array of already-received part indexes (for resume)
};

export type ProgressEventPayload = {
    file: File;
    uid: string;
    uploadedBytes: number;
    totalBytes: number;
    percent: number;          // 0..100 rounded
};

export type FileCompletePayload = {
    file: File;
    uid: string;
};

class SimpleEmitter<T extends string = string> {
    private listeners = new Map<T, Array<(...args: any[]) => void>>();

    on(event: T, cb: (...args: any[]) => void) {
        const arr = this.listeners.get(event) ?? [];
        arr.push(cb);
        this.listeners.set(event, arr);
        return () => this.off(event, cb);
    }

    off(event: T, cb: (...args: any[]) => void) {
        const arr = this.listeners.get(event);
        if (!arr) return;
        this.listeners.set(event, arr.filter(f => f !== cb));
    }

    emit(event: T, ...args: any[]) {
        const arr = this.listeners.get(event);
        if (!arr) return;
        for (const cb of arr.slice()) cb(...args);
    }
}

export class FileUploadTask {
    readonly file: File;
    readonly uid: string;
    readonly chunkSize: number;
    readonly totalChunks: number;
    private config: UploaderConfig;
    private aborted = false;
    private uploadedBytes = 0;
    private uploadedPartsSet = new Set<number>(); // parts already uploaded (includes resumed)

    // small emitter for per-file events
    readonly events = new SimpleEmitter<'progress' | 'error' | 'complete'>();

    constructor(file: File, uid: string, uploadedParts: number[] | undefined, config: UploaderConfig) {
        this.file = file;
        this.uid = uid;
        this.config = config;
        this.chunkSize = config.chunkSize ?? 1024 * 512;
        // totalChunks = ceil(file.size / chunkSize)
        this.totalChunks = Math.ceil(file.size / this.chunkSize);
        if (uploadedParts && uploadedParts.length > 0) {
            uploadedParts.forEach(i => {
                this.uploadedPartsSet.add(i);
                // increase uploadedBytes baseline
                const start = i * this.chunkSize;
                const end = Math.min(start + this.chunkSize, file.size);
                this.uploadedBytes += (end - start);
            });
        }
    }

    get uploaded() { return this.uploadedBytes; }
    get total() { return this.file.size; }
    get percent() {
        return Math.round((this.uploadedBytes / this.file.size) * 100);
    }

    abort() {
        this.aborted = true;
    }

    isPartUploaded(index: number) {
        return this.uploadedPartsSet.has(index);
    }

    markPartUploaded(index: number, bytes: number) {
        if (!this.uploadedPartsSet.has(index)) {
            this.uploadedPartsSet.add(index);
            this.uploadedBytes += bytes;
        }
        this.events.emit('progress', {
            file: this.file,
            uid: this.uid,
            uploadedBytes: this.uploadedBytes,
            totalBytes: this.file.size,
            percent: this.percent
        } as ProgressEventPayload);
    }

    // upload a single chunk using XMLHttpRequest so we can get upload progress events
    uploadChunk(index: number, signal?: AbortSignal): Promise<void> {
        if (this.aborted) return Promise.reject(new Error('Task aborted'));

        // if that part is already uploaded (resume), skip
        if (this.isPartUploaded(index)) return Promise.resolve();

        const start = index * this.chunkSize;
        const end = Math.min(start + this.chunkSize, this.file.size);
        const chunk = this.file.slice(start, end);
        const form = new FormData();
        form.append('meta', JSON.stringify({
            uid: this.uid,
            currentPart: index,
            bytesRead: chunk.size
        }));
        form.append('file', chunk);

        return new Promise<void>((resolve, reject) => {
            const xhr = new XMLHttpRequest();
            let timedOut = false;
            let timerId: number | undefined;

            if (signal) {
                if (signal.aborted) {
                    reject(new Error('aborted'));
                    return;
                }
                // if external abort is signalled, abort xhr
                const onAbort = () => {
                    xhr.abort();
                    reject(new Error('aborted'));
                };
                signal.addEventListener('abort', onAbort, { once: true });
            }

            xhr.open('POST', this.config.uploadUrl, true);

            if (this.config.timeoutMs) {
                timerId = window.setTimeout(() => {
                    timedOut = true;
                    xhr.abort();
                }, this.config.timeoutMs);
            }

            xhr.upload.onprogress = (ev) => {
                // report in-chunk progress if desired; we keep aggregate progress reporting centralized
                // Could emit partial progress here if UI wants smoother updates.
            };

            xhr.onload = () => {
                if (timerId !== undefined) clearTimeout(timerId);
                if (timedOut) {
                    reject(new Error('timeout'));
                    return;
                }
                if (xhr.status >= 200 && xhr.status < 300) {
                    // success — mark bytes as uploaded
                    this.markPartUploaded(index, chunk.size);
                    resolve();
                } else {
                    reject(new Error(`HTTP ${xhr.status}`));
                }
            };

            xhr.onerror = () => {
                if (timerId !== undefined) clearTimeout(timerId);
                reject(new Error('network error'));
            };

            xhr.onabort = () => {
                if (timerId !== undefined) clearTimeout(timerId);
                reject(new Error('aborted'));
            };

            try {
                xhr.send(form);
            } catch (err) {
                if (timerId !== undefined) clearTimeout(timerId);
                reject(err);
            }
        });
    }
}

export class Uploader {
    private config: UploaderConfig;
    readonly events = new SimpleEmitter<'file-progress' | 'file-complete' | 'file-error' | 'error'>();
    private abortControllers = new Map<File, AbortController>();

    constructor(config: UploaderConfig) {
        // set defaults
        this.config = {
            chunkSize: 1024 * 512,
            concurrency: 4,
            maxRetries: 3,
            backoffBaseMs: 500,
            timeoutMs: 0,
            resume: true,
            ...config
        };
        if (!this.config.uploadUrl || !this.config.handshakeUrl) {
            throw new Error('uploadUrl and handshakeUrl required');
        }
    }

    async uploadFiles(files: File[]) {
        // Upload files sequentially or in parallel? We'll upload files sequentially to avoid saturating too many parallel streams.
        // If you want to upload multiple files concurrently, call uploadFile() in parallel externally.
        for (const f of files) {
            try {
                await this.uploadFile(f);
            } catch (err) {
                this.events.emit('file-error', f, err);
            }
        }
    }

    async uploadFile(file: File) {
        // handshake: ask server for uid and optionally existing parts (resume)
        const handshakeResp = await this.handshake(file);
        const task = new FileUploadTask(file, handshakeResp.uid, handshakeResp.uploadedParts, this.config);
        // propagate per-file events to global events
        task.events.on('progress', (payload: ProgressEventPayload) => {
            this.events.emit('file-progress', payload);
        });

        const perFileAbort = new AbortController();
        this.abortControllers.set(file, perFileAbort);

        try {
            await this.uploadTaskWithConcurrency(task, perFileAbort.signal);
            this.events.emit('file-complete', { file: file, uid: task.uid } as FileCompletePayload);
        } catch (err) {
            this.events.emit('file-error', file, err);
            throw err;
        } finally {
            this.abortControllers.delete(file);
        }
    }

    cancelFile(file: File) {
        const c = this.abortControllers.get(file);
        if (c) c.abort();
    }

    private async handshake(file: File): Promise<HandshakeResponse> {
        const resp = await fetch(this.config.handshakeUrl, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                fileName: file.name,
                fileSize: file.size,
                expectedPartSize: this.config.chunkSize,
                totalParts: Math.ceil(file.size / this.config.chunkSize)
            })
        });

        if (!resp.ok) {
            throw new Error(`Handshake failed: ${resp.status}`);
        }
        // expect JSON { uid: string, uploadedParts?: number[] } — fallback to text uid
        const contentType = resp.headers.get('content-type') ?? '';
        if (contentType.includes('application/json')) {
            const json = await resp.json();
            return { uid: json.uid, uploadedParts: json.uploadedParts ?? [] };
        } else {
            const text = await resp.text();
            return { uid: text, uploadedParts: [] };
        }
    }

    private async uploadTaskWithConcurrency(task: FileUploadTask, signal: AbortSignal) {
        const total = task.totalChunks;
        const concurrency = Math.max(1, this.config.concurrency ?? 1);
        let nextIndex = 0;

        // worker function that pulls next chunk index until done
        const worker = async () => {
            while (true) {
                if (signal.aborted) throw new Error('aborted by user');
                // find next index not already uploaded
                let i: number | null = null;
                // fast-forward nextIndex to a part that isn't uploaded
                while (nextIndex < total) {
                    const candidate = nextIndex++;
                    if (!task.isPartUploaded(candidate)) {
                        i = candidate;
                        break;
                    }
                }
                if (i === null) return; // no more parts to upload

                // attempt upload with retries and exponential backoff
                await this.tryUploadWithRetries(task, i, signal);
            }
        };

        // start N workers
        const workers = Array.from({ length: concurrency }, () => worker());
        await Promise.all(workers);
    }

    private async tryUploadWithRetries(task: FileUploadTask, index: number, signal: AbortSignal) {
        const maxRetries = Math.max(0, this.config.maxRetries ?? 3);
        let attempt = 0;

        while (true) {
            if (signal.aborted) throw new Error('aborted by user');

            try {
                // Per-chunk upload uses task.uploadChunk (XHR) so it supports cancellation via signal
                await task.uploadChunk(index, signal);
                return;
            } catch (err) {
                attempt++;
                const retriable = this.isRetriableError(err);
                if (!retriable || attempt > maxRetries) {
                    throw new Error(`Chunk ${index} failed after ${attempt} attempts: ${String(err)}`);
                }
                const backoff = this.exponentialBackoffMs(attempt);
                await this.delay(backoff, signal);
            }
        }
    }

    private isRetriableError(err: any) {
        if (!err) return false;
        const message = String(err).toLowerCase();
        if (message.includes('aborted') || message.includes('timeout')) return true;
        // treat network errors and 5xx as retriable at chunk level — xhr above returns generic strings
        if (message.includes('network') || message.includes('http 5')) return true;
        return true; // be permissive — we'll ultimately respect maxRetries
    }

    private exponentialBackoffMs(attempt: number) {
        const base = this.config.backoffBaseMs ?? 500;
        // jittered exponential: base * 2^(attempt-1) * (0.8..1.2)
        const pow = Math.pow(2, Math.max(0, attempt - 1));
        const jitter = 0.8 + Math.random() * 0.4;
        return Math.floor(base * pow * jitter);
    }

    private delay(ms: number, signal?: AbortSignal) {
        return new Promise<void>((resolve, reject) => {
            const id = window.setTimeout(() => {
                if (signal) signal.removeEventListener('abort', onAbort);
                resolve();
            }, ms);

            const onAbort = () => {
                clearTimeout(id);
                reject(new Error('aborted'));
            };
            if (signal) signal.addEventListener('abort', onAbort, { once: true });
        });
    }
}

