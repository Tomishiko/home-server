async function loadAnilib() {
    const response = await fetch("https://api.anilibria.tv/v3/title/updates?filter=names,player&limit=10");
    if (!response.ok) {
        throw new Error(`Response status: ${response.status}`);
    }

    const data = await response.json();
    var domList = document.getElementById("AnilibList");
    data.list.forEach((title) => {
        const newElem = document.createElement("li");
        newElem.innerHTML = title.names.ru;
        newElem.classList.add("list-group-item");
        newElem.onclick = async function() {
            await fetch("/api/mpv/new", {
                method: "POST",
                body: this.target.dataset.source
            });
        }
        newElem.dataset.source = "https://" + title.player.host + title.player.list[title.player.episodes.last].hls.fhd;
        domList?.appendChild(newElem);
    })


}
