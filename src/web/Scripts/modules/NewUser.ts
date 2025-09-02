import { PostUser } from '../manage/api.js'

export async function init(component) {
    document.getElementById("registerForm")?.addEventListener("submit", async function(e) {
        e.preventDefault();
        e.stopPropagation();
        const form = document.forms["signIn"];
        const uname = form.elements.uname.value;
        if (form.checkValidity()) {
            const selected = form.elements.role.selectedIndex;
            let [result, msg] = await NewUser(uname,
                form.elements.password.value,
                form.elements.role.options[selected].value,
                form.elements.email.value);

            let modal = document.getElementById("modalNotification");
            if(!modal)
                return
            modal.getElementsByTagName('p')[0].innerHTML = msg;

            if (result) {

                modal.getElementsByTagName("button")[0].addEventListener("click", function() {
                    window.location.href = '/Manager';
                });
                alert(msg);
            }

        }

        form.classList.add("was-validated");

    });

}
async function NewUser(username: string, password: string, role: string | null, email: string | null): Promise<[boolean, string]> {
    // TODO: add field validations
    try {

        let result = await PostUser({
            uname: username,
            password: password,
            role: role?.toLowerCase(),
            email: email
        });
        if (result.ok) {
            let msg = `User ${username} succesfully added to the system!`;
            return [true, msg];
        }
        else {
            let body = await result.text()
            let msg = `Unable to create user. ${body}`;
            return [false, msg];
        }
    } catch (ex) {
        console.error(ex);
        return [false,""];
    }
}
