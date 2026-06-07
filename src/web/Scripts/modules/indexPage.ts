import { Uploader, FileCompletePayload, ProgressEventPayload } from "../index/newFileUpload"
import { ProgressBarCtrl } from '../index/ProgressBars'
import { TableViews } from '../index/types'


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
        chunkSize: 8 * 1024 * 1024,
        concurrency: 4,
        maxRetries: 3,
        backoffBaseMs: 500,
        timeoutMs: 30_000,
        resume: true
    }, token);
    const barCtrl = new ProgressBarCtrl();
    let currentView = TableViews.Public;

    document.getElementById('submitBtn')?.addEventListener("click", async function() {

        const filePicker = document.getElementById('file') as HTMLInputElement;
        currentView = document
            .getElementById('partial_table')?.dataset.visibility as TableViews.Public;
        const isShared = currentView === TableViews.Public;

        if (!filePicker.files) return;

        const files = Array.from(filePicker.files)
        const progressBarArea = document.getElementById('fileBlock');
        if (progressBarArea) {
            files.forEach((x, id) => {
                const bar = barCtrl.createFileProgressBar(x.name);
                progressBarArea.appendChild(bar.container);
            });
        }
        await uploader.uploadFiles(files, isShared);
    })
    document.getElementById('table-container')?.addEventListener(
        "htmx:configRequest", (e) => {
            if ((e.target as HTMLElement).id == "table-container") {
                const detail = (e as CustomEvent).detail;
                detail.parameters['action'] = currentView;
            }
        });

    uploader.events.on('file-progress', (payload: ProgressEventPayload) => {
        barCtrl.updateProgressBar(payload.file.name, payload.percent);
    });

    uploader.events.on('file-complete', (payload: FileCompletePayload) => {
        barCtrl.hideUploadProgressBars(payload.file.name);
        (window as any).htmx.trigger("#table-container", "upload-complete");
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
