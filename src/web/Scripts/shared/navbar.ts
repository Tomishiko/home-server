$('.nav-link').on('click', (e) => {
    if (e.target.hasAttribute('active'))
        return;
    e.target.classList.add('active');
})
