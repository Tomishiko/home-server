import { FetchMoreLogs } from "./api.js";

export async function loadMoreLogs(obj: HTMLElement, appendTo: HTMLElement) {
    FetchMoreLogs(obj.dataset.cursor)
        .then(content => {
            if (content) {

                appendTo.innerHTML += content.tableContent;
                if (!content.cursor) {
                    obj.hidden = true;
                    return;
                }
                obj.dataset.cursor = content.cursor;
            }

        });

}
