import * as ContextMenuLogic from './ContextMenu.js'
import * as api from './api.js'

//Add to jquery plugin
(function($, window) {
    $.fn['contextMenu'] = ContextMenuLogic.contexMenu;

})(jQuery, window)

export function setContext() {
    const tableSelector = '#partial_table tr';
    $(tableSelector).contextMenu({
        menuSelector: "#contextMenu",
        callback: function(invokedOn, selectedMenu) {
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

            };


        },
    });
    $('.actionRow').on('dblclick', async function(e) {
        e.preventDefault();
        await FetchTable(Number.parseInt(e.currentTarget.getAttribute('data-action')))
    });
}

export async function FetchTable(action: number) {

    try {
        const response = await api.GetPartialTable(action);
        if (response.redirected) {
            //window.location.replace(response.url);
            window.location.href = response.redirectUrl;
        }
        const newTable = response.content.then(function(string) {
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

    } catch (e) {
        console.log(e);
    }
}
