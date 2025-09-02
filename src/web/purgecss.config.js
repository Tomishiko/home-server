module.exports = {
    content: ['./Views/**/*.cshtml'],
    css: ['./node_modules/bootstrap/dist/css/bootstrap.min.css'],
    output: 'wwwroot/dist/bootstrap.min.css',
    safelist: {
        greedy: [/modal/],
    }
}
