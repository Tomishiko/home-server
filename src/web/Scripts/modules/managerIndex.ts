import { navbarClickHandler } from "../manage/manageSideBar.js"
import { deleteUserConfirmation } from "../manage/DeleteUser.js"
import Offcanvas from 'bootstrap/js/dist/offcanvas.js'

export function init(component: HTMLElement,optionalData:any) {

    const sidebarItem = component.querySelectorAll('#sidebar-item.nav-link');
    sidebarItem.forEach(x => x.addEventListener("click", navbarClickHandler));


    const offcanvas = component.querySelector("#offcanvasResponsive");
    if (offcanvas) {
        const offcanModel = Offcanvas.getOrCreateInstance(offcanvas);
        component.querySelector("#transparent-button")?.addEventListener('click', () => offcanModel.toggle());
        component.querySelector('#closeOffcanvasBtn')?.addEventListener('click', () => offcanModel.hide());
    }
    //document.querySelector('.modal-footer .btn-secondary').addEventListener("click", function(e) {
    //    document.querySelector('#modalConfirm').modal('hide');
    //});
}
