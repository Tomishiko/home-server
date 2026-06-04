// purgecss.config.js

export const content = ['./Views/**/*.cshtml'];

export const css = [
    './node_modules/bootstrap/dist/css/bootstrap.min.css',
    './node_modules/bootstrap-icons/font/bootstrap-icons.min.css'
];

export const output = 'wwwroot/dist/';

export const safelist = {
  // Standard matches look at exact class name rules
  standard: [
    /^modal-/,
    /^offcanvas-/,
    /^collapse/,
    /^nav-/,
    /^show$/,
    /^fade$/,
    /^collapsing$/,
    /^spinner/,
    /^progress/
  ],
  // Greedy matches protect entire complex selectors (like attribute selectors)
  greedy: [
    /bi/ // ◄ Forces it to keep the crucial [class^="bi-"] font-family blocks
  ]
};
