import { Modules } from "../modules/resolver";

//export async function activateModules(container: HTMLElement | null, optionalData?: any) {
//    if (!container)
//        return;
//    const modules = container.querySelectorAll('[data-module]');
//    for (const el of modules) {
//        const modName = (<HTMLElement>el).dataset.module;
//        try {
//            if (modName && Modules[modName]) {
//                Modules[modName]().then(mod => mod.init(el, optionalData));
//            }
//
//        } catch (err) {
//            console.error(`Module "${modName}" failed to load`, err);
//        }
//    }
//}
export async function activateModules(container: HTMLElement | null, optionalData?: any) {
    if (!container) return;

    const elements = Array.from(container.querySelectorAll<HTMLElement>('[data-module]'));
    if (container.dataset.module) {
        elements.push(container);
    }

    const activationPromises = elements.map(async (el) => {
        const modName = el.dataset.module;

        if (!modName || !Modules[modName]) return;

        try {
            const mod = await Modules[modName]();
            await mod.init(el, optionalData);
        } catch (err) {
            console.error(`Module "${modName}" failed to initialize:`, err);
        }
    });

    await Promise.allSettled(activationPromises);
}
