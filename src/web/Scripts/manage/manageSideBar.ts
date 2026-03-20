// Sidebar

export async function navbarClickHandler(e) {
    e.preventDefault();
    const current: HTMLElement = e.target;
    if (current.classList.contains('active'))
        return;
    document.querySelector("#sidebar-item.nav-link.active")?.classList.remove('active');
    //await loadContent(current.getAttribute("href"))
    current.classList.add('active');
}
// Dynamic table render
export async function loadContent(path) {
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
            const content = document.getElementById('content');
            content.innerHTML = string;
            const scripts = content?.getElementsByTagName('script');
            for(var script in scripts)
                eval(script);
        });


    } catch (ex) {
        alert(`something went wrong ${ex}`);
    }
}
//document.addEventListener("DOMContentLoaded", () => {

//loadContent($('a#sidebar-item.active').prop('href'));
//});
