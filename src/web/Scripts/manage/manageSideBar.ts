// Sidebar interactivity
document.querySelectorAll('#sidebar-item.nav-link')
    .forEach(x => x.addEventListener("click",navbarClickHandler))

async function navbarClickHandler(e) {
    e.preventDefault();
    const current = $(e.target);
    if (current.hasClass('active'))
        return;
    $("#sidebar-item.nav-link.active").removeClass('active');
    await loadContent(current.attr("href"))
    current.addClass('active');
}
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
//document.addEventListener("DOMContentLoaded", () => {

//loadContent($('a#sidebar-item.active').prop('href'));
//});
