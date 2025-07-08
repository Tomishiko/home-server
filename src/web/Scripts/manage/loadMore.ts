$('#loadmoreBtn').bind('click',async function(){
    const last = $('tr').last();
    const content = await fetch(`/manager/logspartialtable?lastitem=${last.data('cursor')}`)
        .then(response=>response.text())
        .then(content=> $('#logTable').append(content));

});
