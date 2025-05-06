// Sidebar interactivity
$('#sidebar-item.nav-link').on('click', async function(e) {
    e.preventDefault();
    const current = $(e.target);
    if (current.hasClass('active'))
        return;
    $("#sidebar-item.nav-link.active").removeClass('active');
    await loadContent(current.attr("href"))
    current.addClass('active');
})
document.addEventListener("DOMContentLoaded", () => {
    loadContent('/Pages/ManageUsers');
});
// Dynamic table render
async function loadContent(path) {
    try {
        const response = await fetch(path, {
            method: 'GET',
            headers: {
                "Accept": "text/html",
                "Content-type": "text/json",
            }
        })
        if (response.redirected) {
            window.location.href = response.url;
            return;
        }
        response.text().then(function(string) {
            $('#content').html(string);
        });


    } catch (ex) {

    }
}
