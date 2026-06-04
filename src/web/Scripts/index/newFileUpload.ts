// @ts-ignore
import md5 from 'blueimp-md5'

export type UploaderConfig = {
    chunkSize?: number;        // bytes per chunk (default fallback 512 KB)
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
    partSize: number;
    windowStart: number;
    bitfield: number;
    isSuccess: boolean;
    description?: string;
};

class AuthorizationError extends Error {
    constructor(msg: string) {
        super(msg);
    }
}

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
    private uploadedPartsSet = new Set<number>();

    readonly events = new SimpleEmitter<'progress' | 'error' | 'complete'>();

    constructor(
        file: File,
        uid: string,
        partSize: number,
        windowStart: number,
        bitfield: number,
        config: UploaderConfig
    ) {
        this.file = file;
        this.uid = uid;
        this.config = config;

        // Prioritize backend's PartSize to ensure byte boundaries match perfectly
        this.chunkSize = partSize > 0 ? partSize : (config.chunkSize ?? 1024 * 512);
        this.totalChunks = Math.ceil(file.size / this.chunkSize);

        // All chunks prior to WindowStart are fully processed by the backend
        const startLimit = Math.min(windowStart, this.totalChunks);
        for (let i = 0; i < startLimit; i++) {
            this.addUploadedPart(i);
        }

        // Evaluate the 32-bit bitfield for chunks within the sliding window
        for (let i = 0; i < 32; i++) {
            if (((bitfield >>> i) & 1) === 1) {
                const chunkIndex = windowStart + i;
                if (chunkIndex < this.totalChunks) {
                    this.addUploadedPart(chunkIndex);
                }
            }
        }
    }

    private addUploadedPart(index: number) {
        if (!this.uploadedPartsSet.has(index)) {
            this.uploadedPartsSet.add(index);
            const start = index * this.chunkSize;
            const end = Math.min(start + this.chunkSize, this.file.size);
            this.uploadedBytes += (end - start);
        }
    }

    get uploaded() { return this.uploadedBytes; }
    get total() { return this.file.size; }
    get percent() {
        return this.file.size > 0 ? Math.round((this.uploadedBytes / this.file.size) * 100) : 0;
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

    async uploadChunk(index: number, signal?: AbortSignal): Promise<void> {
        if (this.aborted) throw new Error('Task aborted');
        if (this.isPartUploaded(index)) return;

        const start = index * this.chunkSize;
        const end = Math.min(start + this.chunkSize, this.file.size);
        const chunk = this.file.slice(start, end);

        const controller = new AbortController();
        const onExternalAbort = () => controller.abort('aborted');

        if (signal) {
            if (signal.aborted) throw new Error('aborted');
            signal.addEventListener('abort', onExternalAbort, { once: true });
        }

        let timerId: number | undefined;
        if (this.config.timeoutMs) {
            timerId = window.setTimeout(() => {
                controller.abort('timeout');
            }, this.config.timeoutMs);
        }

        try {
            const response = await fetch(`${this.config.uploadUrl}/${this.uid}`, {
                method: 'POST',
                body: chunk,
                headers: {
                    'Content-Type': 'application/octet-stream',
                    'X-Part': index.toString(),
                },
                credentials: 'omit',
                signal: controller.signal
            });

            if (!response.ok) {
                throw new Error(`HTTP ${response.status}`);
            }

            this.markPartUploaded(index, chunk.size);

        } catch (err: any) {
            if (err.name === 'AbortError') {
                const reason = controller.signal.reason;
                throw new Error(reason === 'timeout' ? 'timeout' : 'aborted');
            }
            throw new Error('network error');
        } finally {
            if (timerId) clearTimeout(timerId);
            if (signal) signal.removeEventListener('abort', onExternalAbort);
        }
    }
}

export class Uploader {
    private config: UploaderConfig;
    private xsrf: string;
    readonly events = new SimpleEmitter<'file-progress' | 'file-complete' | 'file-error' | 'error'>();
    private abortControllers = new Map<File, AbortController>();

    constructor(config: UploaderConfig, xsrf: string) {
        this.config = {
            chunkSize: 1024 * 512,
            concurrency: 4,
            maxRetries: 3,
            backoffBaseMs: 500,
            timeoutMs: 0,
            resume: true,
            ...config
        };
        this.xsrf = xsrf;
        if (!this.config.uploadUrl || !this.config.handshakeUrl) {
            throw new Error('uploadUrl and handshakeUrl required');
        }
    }

    async uploadFiles(files: File[]) {
        for (const f of files) {
            try {
                await this.uploadFile(f);
            } catch (err) {
                if (err instanceof AuthorizationError) {
                    this.events.emit('error', err);
                    return;
                }
                this.events.emit('file-error', f, err);
            }
        }
    }

    async uploadFile(file: File) {
        const handshakeResp = await this.handshake(file);
        if (!handshakeResp.isSuccess) {
            this.events.emit('error', handshakeResp.description, file);
            return;
        }

        const task = new FileUploadTask(
            file,
            handshakeResp.uid,
            handshakeResp.partSize,
            handshakeResp.windowStart,
            handshakeResp.bitfield,
            this.config
        );

        task.events.on('progress', (payload: ProgressEventPayload) => {
            this.events.emit('file-progress', payload);
        });

        const perFileAbort = new AbortController();
        this.abortControllers.set(file, perFileAbort);

        try {
            await this.uploadTaskWithConcurrency(task, perFileAbort.signal);
            this.events.emit('file-complete', { file: file, uid: task.uid } as FileCompletePayload);
        } catch (err) {
            perFileAbort.abort();
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
        const meta = `${file.name}-${file.lastModified}-${file.size}-${file.type}`;
        const fingerprint = md5(meta);
        const resp = await fetch(this.config.handshakeUrl, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'X-XSRF-TOKEN': this.xsrf
            },
            body: JSON.stringify({
                fileName: file.name,
                fileSize: file.size,
                fileFingerprint: fingerprint
            })
        });

        if (!resp.ok) {
            if (resp.status == 401)
                throw new AuthorizationError(`Unauthorized ${resp.status}`);

            return { uid: '', partSize: 0, windowStart: 0, bitfield: 0, isSuccess: false, description: resp.statusText };
        }

        const contentType = resp.headers.get('content-type') ?? '';
        if (contentType.includes('application/json')) {
            const json = await resp.json();

            // Handles both camelCase and PascalCase serializations safely
            return {
                uid: json.uuid ?? json.Uuid ?? '',
                partSize: json.partSize ?? json.PartSize ?? 0,
                windowStart: json.windowStart ?? json.WindowStart ?? 0,
                bitfield: json.bitfield ?? json.Bitfield ?? 0,
                isSuccess: true
            };
        } else {
            const text = await resp.text();
            return { uid: text, partSize: 0, windowStart: 0, bitfield: 0, isSuccess: true };
        }
    }

    private async uploadTaskWithConcurrency(task: FileUploadTask, signal: AbortSignal) {
        const total = task.totalChunks;
        const concurrency = Math.max(1, this.config.concurrency ?? 1);
        let nextIndex = 0;

        const worker = async () => {
            while (true) {
                if (signal.aborted) throw new Error('aborted by user');

                let i: number | null = null;
                while (nextIndex < total) {
                    const candidate = nextIndex++;
                    if (!task.isPartUploaded(candidate)) {
                        i = candidate;
                        break;
                    }
                }
                if (i === null) return;

                await this.tryUploadWithRetries(task, i, signal);
            }
        };

        const workers = Array.from({ length: concurrency }, () => worker());
        await Promise.all(workers);
    }

    private async tryUploadWithRetries(task: FileUploadTask, index: number, signal: AbortSignal) {
        const maxRetries = Math.max(0, this.config.maxRetries ?? 3);
        let attempt = 0;

        while (true) {
            if (signal.aborted) throw new Error('aborted by user');

            try {
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
        if (message === 'error: aborted') return false;
        if (message.includes('aborted') || message.includes('timeout')) return true;
        if (message.includes('network') || message.includes('http 5')) return true;
        return false;
    }

    private exponentialBackoffMs(attempt: number) {
        const base = this.config.backoffBaseMs ?? 500;
        const pow = Math.pow(2, Math.max(0, attempt - 1));
        const jitter = 0.8 + Math.random() * 0.4;
        return Math.floor(base * pow * jitter);
    }

    private delay(ms: number, signal?: AbortSignal) {
        return new Promise<void>((resolve, reject) => {
            if (signal?.aborted) return reject(new Error('aborted'));
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
