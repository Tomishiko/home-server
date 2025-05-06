import * as fu from "./FileUploader.js"

$('#submitBtn').on('click', function() {
    const filePicker = $('#file');
    const progressArea = $('#fileBlock').get()[0];
    const uploader = new fu.FileUploader(filePicker.prop("files"), progressArea);
    uploader.StartStreaming();
})
