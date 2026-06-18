import { navbarClickHandler } from "../manage/manageSideBar.js"
import { deleteUserConfirmation } from "../manage/DeleteUser.js"
import Offcanvas from 'bootstrap/js/dist/offcanvas.js'
import QRCode from "../shared/qrcode.js";

export function init(component: HTMLElement, optionalData: any) {

    const sidebarItem = component.querySelectorAll('#sidebar-item.nav-link');
    sidebarItem.forEach(x => x.addEventListener("click", navbarClickHandler));


    const offcanvas = component.querySelector("#offcanvasResponsive");
    if (offcanvas) {
        const offcanModel = Offcanvas.getOrCreateInstance(offcanvas);
        component.querySelector("#transparent-button")?.addEventListener('click', () => offcanModel.toggle());
        component.querySelector('#closeOffcanvasBtn')?.addEventListener('click', () => offcanModel.hide());
    }
    const qrContainer = document.getElementById('qrcode');
    const qrcode = new QRCode(qrContainer, {
        width: 128,
        height: 128,
        colorDark: "#000000",
        colorLight: "#ffffff"
    });

    const dialog = document.getElementById("dialog") as HTMLDialogElement;
    const showBtn = document.getElementById("qrbutton");

    if (!(qrcode && qrContainer)) {
        return;
    }

    qrContainer.addEventListener("click", async (event: MouseEvent) => {
        await navigator.clipboard.writeText(qrContainer.getAttribute("title") ?? "")
            .then(() => {
                const tooltip = document.getElementById("popup-tooltip");
                if (!tooltip) return

                tooltip.style.left = event.clientX + "px";
                tooltip.style.top = (event.clientY - 35) + "px";
                tooltip.classList.add("show");
                setTimeout(() => {
                    tooltip.classList.remove("show");
                }, 2000);

            }).catch(err => {
                console.error("Failed to copy text: ", err);
            });
    });

    if (dialog && showBtn) {
        // Show the dialog
        showBtn.addEventListener("click", () => {
            const task = fetch("/invite/geninvite");
            let link: string;

            task.then(r => {
                if (r.redirected) {
                    window.location.href = r.url;
                    return;
                }
                if (r.ok) {
                    dialog.showModal();
                    return r.json();
                }
                throw new Error(r.statusText);
            }).then(b => {
                link = `${window.location.host}/invite/${b.token}`;
                qrcode.makeCode(link);
            }).catch(err => { alert(err); dialog.close(); });

        });


        // Close the dialog if the backdrop is clicked
        dialog.addEventListener("mousedown", (event) => {
            // Check if the click happened directly on the dialog element
            const dialogDimensions = dialog.getBoundingClientRect();
            if (
                event.clientX < dialogDimensions.left ||
                event.clientX > dialogDimensions.right ||
                event.clientY < dialogDimensions.top ||
                event.clientY > dialogDimensions.bottom
            ) {
                dialog.close();
            }
        });
    }


    //document.querySelector('.modal-footer .btn-secondary').addEventListener("click", function(e) {
    //    document.querySelector('#modalConfirm').modal('hide');
    //});
}
