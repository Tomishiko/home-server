export interface PartialTableResponse {
    redirected: boolean,
    content: Promise<string>,
    redirectUrl: string
}
export async function GetPartialTable(id: number, folder: string): Promise<PartialTableResponse> {

    const postBody = JSON.stringify({
        id: id,
        folder: folder
    });
    const response = await fetch('/partial', {
        method: 'POST',
        body: postBody,
        headers: {
            "Accept": "text/html",
            "Content-type": "text/json",
        }
    });

    return {
        redirected: response.redirected,
        content: response.text(),
        redirectUrl: response.url
    };
}
