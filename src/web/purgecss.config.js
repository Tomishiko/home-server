module.exports = {
    content: ['./Views/**/*.cshtml'],
    css: ['./node_modules/bootstrap/dist/css/bootstrap.min.css',
            './node_modules/bootstrap-icons/font/bootstrap-icons.min.css'],

    output: 'wwwroot/dist/',
    //safelist: {
    //    greedy: [/modal/],
    //    greedy: [/offcanvas/],
    //    greedy: [/collapse/],
    //    greedy: [/collapsing/],
    //}
    safelist: [
        /^modal-/,
        /^offcanvas-/,
        /^collapse/,
        /^nav-/,
        /^show$/,
        /^fade$/,
        /^collapsing$/,
        /^spinner/,
        /^progress/
    ]
}
