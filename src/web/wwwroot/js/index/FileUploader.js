var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
export class FileUploader {
    constructor(files, progressArea) {
        this.chunkSize = 1024 * 512;
        this.files = files;
        this.progressArea = progressArea;
        this.progressBars = new Array(files.length);
    }
    StartStreaming() {
        for (let i = 0; i < this.files.length; i++) {
            this.progressBars[i] = this.NewProgresBar(this.files[i].name, i);
            this.progressArea.append(this.progressBars[i][0]);
            this.StreamFile(this.files[i], this.progressBars[i]);
        }
    }
    StreamFile(file, progressBar) {
        return __awaiter(this, void 0, void 0, function* () {
            //const file: File = input.prop('files')[0];
            //
            let start = 0;
            let chunkCounter = 0;
            let chunkEnd = 0;
            let chunksSent = 0;
            let numberofChunks = Math.ceil(file.size / this.chunkSize);
            progressBar.show();
            //TODO: move api to separate module
            try {
                const response = yield fetch("api/streaming/handshake", {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json"
                    },
                    body: JSON.stringify({
                        fileName: file.name,
                        fileSize: file.size,
                        totalParts: numberofChunks,
                        expectedPartSize: this.chunkSize
                    })
                });
                if (!response.ok) {
                    console.error(`Server returned: {response}`);
                    return;
                }
                let uid = yield response.text();
                //createChunk();
                while (chunkCounter < numberofChunks) {
                    start = chunkEnd;
                    chunkEnd = Math.min(start + this.chunkSize, file.size);
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
            }
            catch (e) {
                console.error(`Error when trying to handshake {e}`);
            }
            function PostData(link, data) {
                return __awaiter(this, void 0, void 0, function* () {
                    var oReq = new XMLHttpRequest();
                    oReq.open('POST', link, true);
                    oReq.onload = () => {
                        if (oReq.status === 200) {
                            chunksSent++;
                            let progr = Math.round((chunksSent / numberofChunks) * 100).toString();
                            let bar = progressBar;
                            bar.css("width", progr.toString() + "%");
                            bar.attr("aria-valuenow", progr);
                            $("text-status").text(progr);
                            if (chunksSent === numberofChunks) {
                                $(".progress").hide();
                                bar.css('width', "0%");
                                bar.attr("aria-valuenow", 0);
                                progressBar[0].remove();
                            }
                        }
                    };
                    oReq.send(data);
                });
            }
        });
    }
    NewProgresBar(fname, id) {
        const progressBarHTML = `<div class="progress" style="width: 320px;margin-top: 10px;">
                                    <p>${fname}</p>
                                    <div class="progress-bar id=${id} progress-bar-striped progress-bar-animated" role="progressbar" aria-valuenow="0"
                                     aria-valuemin="0" aria-valuemax="100" style="width: 0%"></div>
                                </div>`;
        return $(progressBarHTML);
    }
}
