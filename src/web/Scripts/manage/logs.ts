import { loadMoreLogs } from "./loadMore"
export async function init() {
    document.getElementById("loadmoreBtn").addEventListener("click", async function(eventarg) {
        const rows = document.getElementsByTagName("tr");
        const last = rows[rows.length - 1];
        const uppendto = document.getElementById("logTable");
        if(uppendto)
            await loadMoreLogs(last, uppendto);
    });
}
