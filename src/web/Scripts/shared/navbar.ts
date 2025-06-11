$('header .nav-link').on('click', (e) => {
    if (e.target.hasAttribute('active'))
        return;
    $('header .nav-link').removeClass('active');
    e.target.classList.add('active');
})
