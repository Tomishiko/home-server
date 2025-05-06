var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
$(".progress").hide();
"use strict";
const chunkSize = 1024 * 512;
var start = 0;
var chunkCounter = 0;
var chunkEnd = 0;
var uid = "";
$('#submitBtn').on("click", StreamFile);
function StreamFile() {
    return __awaiter(this, void 0, void 0, function* () {
        const input = $("#file");
        if (input.prop('files').length == 0)
            return;
        const file = input.prop('files')[0];
        var chunksSent = 0;
        var numberofChunks = Math.ceil(file.size / chunkSize);
        $('.progress').show();
        var request = $.post("api/streaming/handshake", {
            fileName: file.name,
            fileSize: file.size,
            totalParts: numberofChunks,
            expectedPartSize: chunkSize
        });
        request.done(function (data) {
            return __awaiter(this, void 0, void 0, function* () {
                uid = data;
                //createChunk();
                while (chunkCounter < numberofChunks) {
                    start = chunkEnd;
                    chunkEnd = Math.min(start + chunkSize, file.size);
                    const chunk = file.slice(start, chunkEnd);
                    const form = new FormData();
                    const meta = {
                        "uid": uid,
                        "currentPart": chunkCounter,
                        "bytesRead": chunkEnd - start
                    };
                    form.append("meta", JSON.stringify(meta));
                    form.append("file", chunk);
                    PostData("api/streaming/uploadlarge", form);
                    chunkCounter++;
                }
                function PostData(link, data) {
                    return __awaiter(this, void 0, void 0, function* () {
                        var oReq = new XMLHttpRequest();
                        oReq.open('POST', link, true);
                        oReq.onload = () => {
                            if (oReq.status === 200) {
                                chunksSent++;
                                let progr = Math.round((chunksSent / numberofChunks) * 100).toString();
                                let bar = $('.progress-bar');
                                bar.css("width", progr.toString() + "%");
                                bar.attr("aria-valuenow", progr);
                                $("text-status").text(progr);
                                if (chunksSent === numberofChunks) {
                                    $(".progress").hide();
                                    bar.css('width', "0%");
                                    bar.attr("aria-valuenow", 0);
                                    location.reload();
                                }
                            }
                        };
                        oReq.send(data);
                    });
                }
            });
        });
        request.fail(function () {
            alert("error while handshake");
            $(".progress").hide();
        });
    });
}
function sendAbortion(uid) {
    return __awaiter(this, void 0, void 0, function* () {
        var request = $.post('api/streaming/abort', {
            'uid': uid
        });
    });
}
function getCookie(name) {
    var value = "; " + document.cookie;
    var parts = value.split("; " + name + "=");
    if (parts.length == 2)
        return parts.pop().split(";").shift();
}
