import { event, Event } from "jquery";
import { contexMenu } from "./ContextMenu.js"
import { GetPartialTable, DeleteFile } from "./api.js"

export function setContext() {
    const table = document.getElementById('partial_table');
    const menu = document.getElementById('contextMenu');
    if (!menu || !table) return;
    let currntRowId: number = 0;
    table.addEventListener("contextmenu", (e: MouseEvent) => {
        if(e.ctrlKey) return;
        const row = (<HTMLElement>e.target).closest("tr");
        if (!row) return;
        e.preventDefault();
        menu.style.display = 'block';
        const left = checkClipping(e.pageX, menu.offsetWidth, window.innerWidth);
        const right = checkClipping(e.pageY, menu.offsetHeight, window.innerHeight);
        menu.style.left = `${left}px`;
        menu.style.top = `${right}px`;
        const deleteBtn = menu.querySelector<HTMLLinkElement>("#delete");
        //deleteBtn!.href = row.closest("a")?.href ?? "";
        const ref = row.querySelector("a")!.href;
        deleteBtn?.addEventListener("click", async (e) => {
            try {
                const response = await DeleteFile(ref);
                if (response.ok) {
                    location.reload();
                    return;
                }
                response.text().then(s => console.error(`${s}`));
            } catch (ex) {
                console.error(`${ex}`);
            }
        });
    });
    document.addEventListener("click", () => {
        menu.style.display = "none";
    });



    document.querySelectorAll('.actionRow').forEach(x => {
        x.addEventListener('dblclick', async function(e) {
            e.preventDefault();
            const action = (<HTMLElement>e.currentTarget).dataset["action"];
            if (!action) return;
            await FetchTable(Number.parseInt(action));
        })
    });

}

function checkClipping(mouse: number, menu: number, win: number) {
    // opening menu would pass the side of the page
    if (mouse + menu > win && menu < mouse)
        return mouse - menu;
    return mouse;

}
export async function FetchTable(action: number) {

    try {
        const response = await GetPartialTable(action);
        if (response.redirected) {
            //window.location.replace(response.url);
            window.location.href = response.redirectUrl;
        }
        response.content.then(function(string) {
            let tableCont = document.getElementById('table-container');
            if (!tableCont)
                return;
            tableCont.innerHTML = string;
            setContext();
        });


    } catch (e) {
        console.error(e);
    }
}
