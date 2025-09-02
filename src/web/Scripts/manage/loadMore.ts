import { FetchMoreLogs } from "./api.js";

export async function loadMoreLogs(last: HTMLElement,appendElem:HTMLElement) {
    FetchMoreLogs(last.dataset.cursor)
        .then(content => appendElem.innerHTML += content);

}
