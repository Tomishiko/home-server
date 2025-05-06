var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
// Sidebar interactivity
$('#sidebar-item.nav-link').on('click', function (e) {
    return __awaiter(this, void 0, void 0, function* () {
        e.preventDefault();
        const current = $(e.target);
        if (current.hasClass('active'))
            return;
        $("#sidebar-item.nav-link.active").removeClass('active');
        yield loadContent(current.attr("href"));
        current.addClass('active');
    });
});
document.addEventListener("DOMContentLoaded", () => {
    loadContent('/Pages/ManageUsers');
});
// Dynamic table render
function loadContent(path) {
    return __awaiter(this, void 0, void 0, function* () {
        try {
            const response = yield fetch(path, {
                method: 'GET',
                headers: {
                    "Accept": "text/html",
                    "Content-type": "text/json",
                }
            });
            if (response.redirected) {
                window.location.href = response.url;
                return;
            }
            response.text().then(function (string) {
                $('#content').html(string);
            });
        }
        catch (ex) {
        }
    });
}
