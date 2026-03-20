export interface PartialTableResponse {
    redirected: boolean,
    content: Promise<string>,
    redirectUrl: string
}
export async function GetPartialTable(action: number): Promise<PartialTableResponse> {

    const postBody = JSON.stringify({
        action: action,
    });
    const response = await fetch(`/index/partialtableload?action=${action}`, {
        method: 'GET',
        headers: {
            "Accept": "text/html",
            "Content-type": "text/json",
            "X-Requested-With": "XMLHttpRequest"
        }
    });

    return {
        redirected: response.redirected,
        content: response.text(),
        redirectUrl: response.url
    };
}
export function DeleteFile(ref: string): Promise<Response> {
    return fetch(ref, {
        method: "DELETE",

    });
}
