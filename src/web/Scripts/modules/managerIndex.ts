import { navbarClickHandler } from "../manage/manageSideBar.js"
import { deleteUserConfirmation } from "../manage/DeleteUser.js"
import Modal from 'bootstrap/js/dist/modal.js'

export function init(component: HTMLElement) {

    const sidebarItem = component.querySelectorAll('#sidebar-item.nav-link');
    sidebarItem.forEach(x => x.addEventListener("click", navbarClickHandler));


    const modalEl = component.querySelector<HTMLElement>('#modalConfirm');
    if (modalEl) {

        const modalObj = Modal.getOrCreateInstance(modalEl, { keyboard: true, backdrop: true });
        component.querySelector('#confirm')?.addEventListener('click', function() {
            modalObj.hide();
        });
        component.querySelectorAll("a.btn-delete").forEach(el =>
            el.addEventListener("click", (e) => {
                modalObj.show();
                deleteUserConfirmation(e, modalEl);
            }));

    }
    //document.querySelector('.modal-footer .btn-secondary').addEventListener("click", function(e) {
    //    document.querySelector('#modalConfirm').modal('hide');
    //});
}
