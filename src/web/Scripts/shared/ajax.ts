enum Action {
    Substitute = "substitute",
    Append = "append"
};

async function ajaxPartial(action: Action, method: string, contentContainer: HTMLElement, href: string) {
    try {

        const response = await fetch(href, {
            method: method.toUpperCase(),
            headers: {
                "Accept": "text/html",
                "X-Requested-With": "XMLHttpRequest"
            }

        });

        if (response && !response.ok) {
            location.href = href;
            return;
        }
        response.text().then(x => {
            switch (action) {
                case Action.Append:
                    contentContainer.innerHTML += x;
                    break;
                case Action.Substitute:
                    contentContainer.innerHTML = x;
                    break;
            }

            window.mainNavHandler(x, href,contentContainer);
            window.addAjaxListeners(contentContainer);
        });

    } catch (exception) {
        console.error(exception);
    }

}
async function glolbalNvaigationClickHandle(event: Event) {
    event.preventDefault();
    const target = event.target as HTMLElement;
    const containerSelector = target.dataset["ajaxContainer"] ?? "#main";
    const container = document.querySelectorAll(containerSelector)[0] as HTMLHtmlElement;
    const ref = target.dataset["ajaxUrl"];
    const action = target.dataset["ajaxAction"];
    const method = target.dataset["ajaxMethod"];
    if (container && ref && action && method)
        await ajaxPartial(action as Action, method, container, ref);
}
export { Action, ajaxPartial, glolbalNvaigationClickHandle };

