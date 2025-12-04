import { setContext } from '../index/partialTable'
import { Uploader, FileUploadTask, UploaderConfig, FileCompletePayload, ProgressEventPayload } from "../index/newFileUpload"
import { ProgressBarCtrl } from '../index/ProgressBars'
import { removeData } from 'jquery';

export function init(component) {
    // document.getElementById('submitBtn').addEventListener('click', function() {
    //     const filePicker = document.getElementById('file') as HTMLInputElement;
    //     const progressArea = document.getElementById('fileBlock');
    //     const uploader = new FileUploader(filePicker.files, progressArea);
    //     uploader.StartStreaming();
    // });
    const uploader = new Uploader({
        uploadUrl: '/api/streaming/uploadlarge',
        handshakeUrl: '/api/streaming/handshake',
        chunkSize: 512 * 1024,
        concurrency: 4,
        maxRetries: 3,
        backoffBaseMs: 500,
        timeoutMs: 30_000,
        resume: true
    });
    const barCtrl = new ProgressBarCtrl();

    document.getElementById('submitBtn')?.addEventListener("click", function() {

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
        uploader.uploadFiles(files);
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
    uploader.events.on('error', (err: any,object:any) => {
        console.error(err,object);
        alert(`${err}`);
        barCtrl.hideUploadProgressBars()
    });
    // Context menu for main table
    setContext();
}
