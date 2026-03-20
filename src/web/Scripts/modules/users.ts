import Modal from 'bootstrap/js/dist/modal.js'
import { deleteUserConfirmation } from '../manage/DeleteUser.js';
import { event } from 'jquery';
import { Action, ajaxPartial } from '../shared/ajax.js';

export function init(component: HTMLElement, optionalData: any) {

    // const modalEl = document.getElementById("modalConfirm");
    // if (modalEl) {

    //     const modalObj = Modal.getOrCreateInstance(modalEl, { keyboard: true, backdrop: true });
    //     const parent = component.parentNode as HTMLElement;
    //     parent.addEventListener("click", (e: MouseEvent) => {
    //         const target = e.target as HTMLElement;
    //         if (target && target.matches('a.btn-delete')) {

    //             e.preventDefault();
    //             modalObj.show();
    //             deleteUserConfirmation(target, modalEl, modalObj);
    //         }
    //     });

        // const modalAccept = modalEl.querySelector('.modal-footer button#confirm')!;
        // modalAccept.addEventListener("click", async (event) => {
        //     try {
        //         const ref = (event.target as HTMLElement).dataset["ref"];
        //         if (!ref) { return; }

        //         const response = await fetch(ref, { method: "DELETE" });
        //         if (response.ok) {
        //             const containerSelector = "#content";
        //             const container = document.querySelectorAll(containerSelector)[0] as HTMLHtmlElement;
        //             const ref = "/manager/manageusers";
        //             const action = "substitute";
        //             const method = "GET";
        //             await ajaxPartial(action as Action, method, container, ref, false);
        //             //await activateModules(document.querySelectorAll(containerSelector)[0] as HTMLHtmlElement);

        //             //location.reload();
        //         }

        //     } catch (ex) {
        //         console.error(ex);
        //         alert("something went wrong");
        //     }
        //     finally {
        //         modalObj.hide();
        //     }
        // })

        //component.querySelectorAll("a.btn-delete").forEach(el =>
        //    el.addEventListener("click", (e) => {
        //        modalObj.show();
        //        deleteUserConfirmation(e, modalEl,modalObj);
        //    }));

    //}
}
