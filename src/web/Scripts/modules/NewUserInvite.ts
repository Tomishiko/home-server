export async function init(component, optionalData: any) {
    document.getElementById("registerForm")?.addEventListener("submit", async function(e) {
        e.preventDefault();
        e.stopPropagation();
        const form = document.forms["signIn"];
        const formData = new FormData(form);
        //const uname = form.elements.uname.value;
        if (form.checkValidity()) {
            const result = await fetch("register", {
                method: "POST",
                body: formData
            });

            let modal = document.getElementById("modalNotification");
            if (!modal)
                return
            let msg;
            const modalText = modal.getElementsByTagName('p')[0];
            modalText.innerHTML = msg;

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
