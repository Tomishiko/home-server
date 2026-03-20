import { activateModules } from "./shared/activateModules";
import Collapse from 'bootstrap/js/dist/collapse';

document.querySelectorAll('nav a.nav-link').forEach(x => x.addEventListener('click', function() {
    document.getElementById('#navbar-main')?.collapse('hide');
}));
// handling back/forward navigation
// initial module load
document.addEventListener('DOMContentLoaded', async () => {
    const content = document.getElementById("main");
    const utcOffset = Intl.DateTimeFormat().resolvedOptions().timeZone;
    document.cookie = `utcOffset=${utcOffset}`;

    const navElement = document.getElementById('navbar-main'); // The ID of your collapsible div
    const navToggle = document.querySelector('.navbar-toggler');

    if (navElement && navToggle) {
        const bsCollapse = new Collapse(navElement, { toggle: false });

        navToggle.addEventListener('click', () => {
            bsCollapse.toggle();
        });

        const navLinks = navElement.querySelectorAll('.nav-link');
        navLinks.forEach((link) => {
            link.addEventListener('click', () => {
                // Only collapse if the menu is currently visible (to avoid flickering)
                if (navElement.classList.contains('show')) {
                    bsCollapse.hide();
                }
            });
        });
    }
});

document.addEventListener("htmx:load", async function(e: any) {
    await activateModules(e.detail.elt);
});
document.addEventListener("htmx:afterOnLoad", (e: any) => {

    const target = e.detail.elt as HTMLElement;
    if (!target.matches(".nav-link.navbar-element")) {
        return;
    }
    document.querySelector('header .nav-link.navbar-element.active')?.classList.remove('active');


    target.classList.add('active');
});

document.addEventListener('htmx:configRequest', (evt: any) => {
    const tokenInBody = evt.detail.parameters['__RequestVerificationToken'];

    if (!tokenInBody && evt.detail.verb !== 'get') {
        const token = (document.querySelector('input[name="__RequestVerificationToken"]') as HTMLInputElement).value;

        if (token) {
            evt.detail.headers['X-XSRF-TOKEN'] = token;
        }
    }
});
document.addEventListener("htmx:responseError", (e: any) => {
    const xhr = e.detail.xhr as XMLHttpRequest;
    alert(`${xhr.statusText}\n${xhr.responseText}`);
});
