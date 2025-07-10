//$('a.btn-outline-danger').on('click',deleteUserConfirmation);

export async function deleteUserConfirmation(e){

    e.preventDefault();
    const user = e.currentTarget.dataset.tag;
    const modal = $('#modalConfirm');
    const ref = e.currentTarget.href;
    $('div.modal-body p').html(`Are you sure you want to DELETE ${user} and all private files associated with this user?`);
    const modalAccept = $('.modal-footer button#confirm');
    modalAccept.off();
    modalAccept.click(async function(btn) {
        try {
            const response = await fetch(ref, { method: "DELETE" });
            if (response.ok) {
                location.reload();
            }

        } catch (ex) {
            alert(ex);
        }
    })


    modal.modal('show');
}
