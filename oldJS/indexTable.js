async function FetchTable(id) {
    const postBody = JSON.stringify({
        id: id,
        folder: $('#breadcrumbs').html()
    })

    try {
        const response = await fetch('/partial', {
            method: 'POST',
            body: postBody,
            headers: {
                "Accept": "text/html",
                "Content-type": "text/json",
            }
        })
        if (response.redirected) {
            //window.location.replace(response.url);
            window.location.href = response.url;
            return;
        }
        response.text().then(function(string) {
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
