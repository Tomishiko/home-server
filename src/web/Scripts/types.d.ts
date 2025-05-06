//For jquery plugin
interface JQuery {
    contextMenu(settings: ContextSetting): any;
}
type ContexMenuCallback = (a: JQuery, b: JQuery) => void;
interface ContextSetting {
    menuSelector: string,
    callback: ContexMenuCallback
}
