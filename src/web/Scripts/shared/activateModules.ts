import { Modules } from "../modules/resolver";

export async function activateModules(container: HTMLElement | null, optionalData?: any) {
    if (!container)
        return;
    const modules = container.querySelectorAll('[data-module]');
    for (const el of modules) {
        const modName = (<HTMLElement>el).dataset.module;
        try {
            if (modName && Modules[modName]) {
                Modules[modName]().then(mod => mod.init(el, optionalData));
            }

        } catch (err) {
            console.error(`Module "${modName}" failed to load`, err);
        }
    }
}
