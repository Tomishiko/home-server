var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
import * as ContextMenuLogic from './ContextMenu.js';
import * as api from './api.js';
//Add to jquery plugin
(function ($, window) {
    $.fn['contextMenu'] = ContextMenuLogic.contexMenu;
})(jQuery, window);
function setContext() {
    const tableSelector = '#partial_table tr';
    $(tableSelector).contextMenu({
        menuSelector: "#contextMenu",
        callback: function (invokedOn, selectedMenu) {
            switch (selectedMenu[0]) {
                case $("#contextMenu #download")[0]:
                    alert("download");
                    break;
                case $("#contextMenu #print")[0]:
                    alert("pirnt request");
                    break;
                    var url = invokedOn.find('a').prop('href');
                    url = "/api/pfile" + url.substring(url.lastIndexOf('/'));
                    fetch(url + '?printParams=printingparams');
                    break;
            }
            ;
        },
    });
    $('tr#name').on('dblclick', function (e) {
        return __awaiter(this, void 0, void 0, function* () {
            e.preventDefault();
            yield FetchTable(Number(e.currentTarget.getAttribute('data-index')));
        });
    });
    $('#backBtn').on('dblclick', function (e) {
        return __awaiter(this, void 0, void 0, function* () {
            e.preventDefault();
            yield FetchTable(-1);
        });
    });
}
document.addEventListener("DOMContentLoaded", () => {
    setContext();
});
export function FetchTable(id) {
    return __awaiter(this, void 0, void 0, function* () {
        try {
            const response = yield api.GetPartialTable(id, $('#breadcrumbs').html());
            if (response.redirected) {
                //window.location.replace(response.url);
                window.location.href = response.redirectUrl;
                return;
            }
            const newTable = response.content.then(function (string) {
                $('#table-container').html(string);
                setContext();
            });
            //var request = $.post('partial',
            //    {
            //        'id': id,
            //        'folder': $('#breadcrumbs').html()
            //    }
            //);
            //request.done(function (data) {
            //    $('#table-container').html(data);
            //})
            //request.fail(function () {
            //    alert('failed to get table data');
            //})
        }
        catch (e) {
            console.log(e);
        }
    });
}
