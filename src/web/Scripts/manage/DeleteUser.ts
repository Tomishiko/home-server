//$('a.btn-outline-danger').on('click',deleteUserConfirmation);
//
export async function deleteUserConfirmation(e: Event, modalElement: HTMLElement) {

    e.preventDefault();
    const target = e.currentTarget as HTMLElement;
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

    const modalAccept = modalElement.querySelector('.modal-footer button#confirm')!;

    modalAccept.addEventListener("click", async function(btn) {
        try {
            const response = await fetch(ref, { method: "DELETE" });
            if (response.ok) {
                location.reload();
            }

        } catch (ex) {
            console.error(ex);
            alert("something went wrong");
        }
    })

}
