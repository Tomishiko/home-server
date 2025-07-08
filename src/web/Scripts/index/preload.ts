import {FileUploader}from "./FileUploader.js"

$('#submitBtn').on('click', function() {
    const filePicker = $('#file');
    const progressArea = $('#fileBlock').get()[0];
    const uploader = new FileUploader(filePicker.prop("files"), progressArea);
    uploader.StartStreaming();
})
