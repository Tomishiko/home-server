import { Modal } from 'bootstrap';
import { FileApiClient } from '../Api/Client.js'
import { Action, ajaxPartial } from '../shared/ajax.js'
import { activateModules } from '../shared/activateModules.js';

export async function deleteUserConfirmation(target: HTMLElement, modalElement: HTMLElement, modalObj: Modal) {

    const user = target.dataset["tag"];
    const ref = target.getAttribute("href");
    if (!ref) {
        console.error(`href is not specified for ${target}`);
        return;
    }
    if (!user) {

        console.error(`data-tag is not specified for ${target}`);
        return;
    }
    modalElement.querySelector('.modal-body p')!.innerHTML = `Are you sure you want to DELETE ${user} and all private files associated with this user?`;
    const modalAccept = modalElement.querySelector('.modal-footer button#confirm')! as HTMLElement;
    modalAccept.dataset["ref"] = ref;
    modalAccept.setAttribute("hx-delete", ref);

}
