import { Modules } from "./modules/resolver";
import * as ajax from "./shared/ajax"

declare global {
    interface Window { mainNavHandler: Function; subNavHandler: Function; addAjaxListeners: Function }
}

document.querySelectorAll('nav a.nav-link').forEach(x => x.addEventListener('click', function() {
    document.getElementById('#navbar-main')?.collapse('hide');
}));
// handling back/forward navigation
window.addEventListener('popstate', async function(e) {
    if (e.state && e.state.html) {
        document.getElementsByTagName('main')[0].innerHTML = e.state.html;
        document.querySelector('header .nav-link.active')?.classList.remove('active');
        document.querySelectorAll('header .nav-link')[e.state.active].classList.add('active');
        //$('header .nav-link').removeClass('active');
        //$('header .nav-link')[e.state.active].classList.add("active");
    } else if (e.state && e.state.url) {
        const response = await fetch(e.state.url);
        if (response.status != 200) {
            this.alert(`Something unexpected happend ${response.statusText}`);
            return;
        }
        document.getElementsByTagName('main')[0].innerHTML = await response.text();
    } else {
        // Fallback: reload
        location.reload();
    }
});
// initial module load
document.addEventListener('DOMContentLoaded', async () => {
    const content = document.getElementById("main");
    window.addAjaxListeners(document.body);
    await activateModules(content);

});
// main navbar class manipulation on navigation
document.querySelectorAll('header .nav-link')
    .forEach(x => x.addEventListener('click', (e) => {
        const target = e.target as HTMLElement;
        if (target.hasAttribute('active'))
            return;

        document.querySelectorAll('header .nav-link')
            .forEach(x => x.classList.remove('active'));
        target.classList.add('active');
    }));

async function activateModules(container: HTMLElement | null) {
    if (!container)
        return;
    const modules = container.querySelectorAll('[data-module]');
    for (const el of modules) {
        const modName = (<HTMLElement>el).dataset.module;
        try {
            if (modName && Modules[modName]) {
                Modules[modName]().then(mod => mod.init(el));
            }

        } catch (err) {
            console.error(`Module "${modName}" failed to load`, err);
        }
    }
}
window.addAjaxListeners = function addAjaxListeners(container: HTMLElement) {

    const navigational = container.querySelectorAll('[data-ajax]');
    for (const el of navigational) {
        el.addEventListener("click", ajax.glolbalNvaigationClickHandle);
    }
}


window.mainNavHandler = async function partialSuccess(data: string, href: string, container: HTMLElement) {
    const newContent = data;
    const newUrl: string = href;
    //const active:number = $('header .nav-link.active').index("header .nav-link");
    var active = 0;
    document.querySelectorAll("header .nav-link").forEach((node, index) => {
        if (node.classList.contains("active"))
            active = index;
    });
    if (newUrl) {
        history.pushState({ html: newContent, url: newUrl, active: active }, '', newUrl);
    }
    await activateModules(container);
}
window.subNavHandler = async function subnavigationSuccess(data: string, status, xhr) {
    activateModules(document.getElementById("content"));
}
