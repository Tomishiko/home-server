export interface ContextSetting {
    menuSelector: string,
    callback: Function,
}
export function contexMenu(settings: ContextSetting) {
    return this.each(function() {
        // Open context menu
        this.addEventListener("contextmenu", function(e) {
            // return native menu if pressing control
            if (e.ctrlKey) return

            //open menu
            var menu = document.querySelector(settings.menuSelector) as HTMLElement;
            menu.dataset["invokedOn"] = e.target;


            menu.style = JSON.stringify({
                position: "absolute",
                display: "block",
                left: getMenuPosition(e.clientX, "width", "scrollLeft"),
                top: getMenuPosition(e.clientY, "height", "scrollTop"),
            });
            menu.querySelectorAll("a").forEach(node => {
                node.addEventListener("click", function(e) {
                    menu.style.display = "none";
                    var invokedOn = menu.dataset["invokedOn"];
                    var selectedMenu = e.target;
                    settings.callback.call(this, invokedOn, selectedMenu);
                })
            })

            return false
        })

        //make sure menu closes on any click
        document.body.addEventListener("click", function() {
            let menu = document.querySelector(settings.menuSelector) as HTMLElement;
            menu.style.display = "none;"
        })
    })

}

