import { Modal } from 'bootstrap';
import { PostUser } from '../manage/api.js'
import { ErrorDto, ManagerApiClient, RegisterManagerRequest } from '../Api/Client.js';

export async function init(component, optionalData: any) {
    document.getElementById("registerForm")?.addEventListener("submit", async function(e) {
        e.preventDefault();
        e.stopPropagation();
        const form = document.forms["signIn"];
        const uname = form.elements.uname.value;
        const client = new ManagerApiClient();
        if (form.checkValidity()) {
            const selected = form.elements.role.selectedIndex;
            const request = new RegisterManagerRequest(
                {
                    username: uname,
                    password: form.elements.password.value,
                    email: form.elements.email.value,
                    role: form.elements.role.options[selected].value
                });

            const modal = document.getElementById("modalNotification");
            let msg: string = "";
            let showModal = true;

            try {

                const userDto = await client.userPOST(request);
                msg = `User ${userDto.username} is succesfully created`;
                modal?.addEventListener("hidden.bs.modal", () => {
                    window.location.href = '/Manager';
                });


            } catch (error) {
                if (error.status === 400) {
                    console.error("Validation failed:", error);
                    showModal = false;
                }
                else if (error.status === 401) {
                    msg = "Unauthorized: Please log in again.";
                    modal?.addEventListener("hidden.bs.modal", () => {
                        window.location.href = '/Login';
                    });
                }
                else if (error.status === 409) {

                    msg = `${error.message}`;
                }
                else {
                    msg = error;
                    console.error(msg);
                    showModal = false;
                }


                if (modal) {
                    setModalText(modal, msg);
                }
                else alert(msg);

            }

            //let [result, msg] = await NewUser(uname,
            //    form.elements.password.value,
            //    form.elements.role.options[selected].value,
            //    form.elements.email.value);

            if (modal && showModal) {
                modal.getElementsByTagName('p')[0].innerHTML = msg;
                const modalObj = new Modal(modal, { backdrop: true, keyboard: true });
                modalObj.show();
            }
        }

        form.classList.add("was-validated");

    });

}
function setModalText(modal: HTMLElement, text: string) {


    modal.getElementsByTagName('p')[0].innerHTML = text;
    const textArea = modal.querySelector(".modal-body p");
    if (textArea) textArea.innerHTML = text;
}
