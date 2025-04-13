(function($, window) {
    $.fn.contextMenu = function(settings) {
        return this.each(function() {
            // Open context menu
            $(this).on("contextmenu", function(e) {
                // return native menu if pressing control
                if (e.ctrlKey) return

                //open menu
                var $menu = $(settings.menuSelector)
                    .data("invokedOn", $(e.currentTarget))
                    .show()
                    .css({
                        position: "absolute",
                        left: getMenuPosition(e.clientX, "width", "scrollLeft"),
                        top: getMenuPosition(e.clientY, "height", "scrollTop"),
                    })
                    .off("click")
                    .on("click", "a", function(e) {
                        $menu.hide()

                        var $invokedOn = $menu.data("invokedOn")
                        var $selectedMenu = $(e.target)

                        settings.menuSelected.call(this, $invokedOn, $selectedMenu)
                    })

                return false
            })

            //make sure menu closes on any click
            $("html").click(function() {
                $(settings.menuSelector).hide()
            })
        })

        function getMenuPosition(mouse, direction, scrollDir) {
            var win = $(window)[direction](),
                scroll = $(window)[scrollDir](),
                menu = $(settings.menuSelector)[direction](),
                position = mouse + scroll

            // opening menu would pass the side of the page
            if (mouse + menu > win && menu < mouse) position -= menu

            return position
        }
    }
})(jQuery, window)

function setContext() {


    $("#partial_table tr").contextMenu({
        menuSelector: "#contextMenu",
        menuSelected: function(invokedOn, selectedMenu) {
            switch (selectedMenu[0]) {
                case $("#contextMenu #download")[0]:
                    alert("download");
                    break;
                case $("#contextMenu #print")[0]:
                    var url = invokedOn.find('a').prop('href');
                    url = "/api/pfile" + url.substring(url.lastIndexOf('/'));
                    fetch(url + '?printParams=printingparams');
                    break;

            };


        },
    })
}
document.addEventListener("DOMContentLoaded", () => {
    setContext();
});
