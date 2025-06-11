//function partialSuccess (data, status, xhr) {
//    const newContent = data; // assuming your server returns partial HTML
//
//    history.pushState({ html: newContent }, '');
//}
function partialSuccess(data: string, status, xhr) {
    const newContent = data; // assuming your server returns partial HTML
    const newUrl: string = this.getAttribute('href');
    const active:number = $('header .nav-link.active').index("header .nav-link");
    if (newUrl) {
        history.pushState({ html: newContent, url: newUrl,active:active}, '', newUrl);
    }
}

window.addEventListener('popstate', function(e) {
    if (e.state && e.state.html) {
        $('main').html(e.state.html);
        $('header .nav-link').removeClass('active');
        $('header .nav-link')[e.state.active].classList.add("active");
    } else if (e.state && e.state.url) {
        // Optional: refetch via AJAX if you didn't store HTML
        $.get(e.state.url, function(data) {
            $('#your-partial-container').html(data);
        });
    } else {
        // Fallback: reload
        location.reload();
    }
});
