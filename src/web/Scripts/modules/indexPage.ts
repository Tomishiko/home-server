import { FetchTable, setContext } from '../index/partialTable'
import { Uploader, FileUploadTask, UploaderConfig, FileCompletePayload, ProgressEventPayload } from "../index/newFileUpload"
import { ProgressBarCtrl } from '../index/ProgressBars'

export function init(component: HTMLElement, optionalData: any) {
    // document.getElementById('submitBtn').addEventListener('click', function() {
    //     const filePicker = document.getElementById('file') as HTMLInputElement;
    //     const progressArea = document.getElementById('fileBlock');
    //     const uploader = new FileUploader(filePicker.files, progressArea);
    //     uploader.StartStreaming();
    // });
    const element = document.getElementsByName("__RequestVerificationToken")[0] as HTMLInputElement;
    const token = element ? element.value : "";
    const uploader = new Uploader({
        uploadUrl: '/api/upload/part',
        handshakeUrl: '/api/upload/handshake',
        chunkSize: 8 * 1024 * 1024,//4096 * 1024, // 4 Mb
        concurrency: 4,
        maxRetries: 3,
        backoffBaseMs: 500,
        timeoutMs: 30_000,
        resume: true
    }, token);
    const barCtrl = new ProgressBarCtrl();

    document.getElementById('submitBtn')?.addEventListener("click",async function() {

        const filePicker = document.getElementById('file') as HTMLInputElement;
        if (!filePicker.files) return;
        const files = Array.from(filePicker.files)
        const progressBarArea = document.getElementById('fileBlock');
        if (progressBarArea) {
            files.forEach((x, id) => {
                const bar = barCtrl.createFileProgressBar(x.name);
                progressBarArea.appendChild(bar.container);
            });
        }
        await uploader.uploadFiles(files);
        (window as any).htmx.trigger("body", "upload-complete");
    })
    uploader.events.on('file-progress', (payload: ProgressEventPayload) => {
        barCtrl.updateProgressBar(payload.file.name, payload.percent);
    });

    uploader.events.on('file-complete', (payload: FileCompletePayload) => {
        barCtrl.hideUploadProgressBars(payload.file.name);
    });


    uploader.events.on('file-error', (file: File, err: any) => {
        console.error(err, file);
        alert(`Unexpected error when uploading file ${file.name}`);
        barCtrl.hideUploadProgressBars(file.name);
    });
    uploader.events.on('error', (err: any, object: any) => {
        console.error(err, object);
        alert(`${err}`);
        barCtrl.hideUploadProgressBars()
    });
}
