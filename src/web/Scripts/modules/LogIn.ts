import {
    ApiException,
    AuthenticationApiClient,
    ErrorDto,
    ProblemDetails
} from '../Api/Client.js'
import { getCookie } from '../utils.js'

export function init(component: HTMLElement, optionalData: any) {
    const form = component.querySelector("form");
  //  form?.addEventListener('submit', onSubmitHandle);
}

async function onSubmitHandle(e: SubmitEvent) {
    e.preventDefault();
    const form = e.target as HTMLFormElement;
    const formData = new FormData(form);
    const client = new AuthenticationApiClient();
    const { Username, Password} = Object.fromEntries(formData.entries());
    //const token = formData.get("__RequestVerificationToken");
    try {
        await client.auth(Username as string, Password as string);
        const queryString: string = window.location.search;
        const urlParams = new URLSearchParams(queryString);
        var returnUrl = urlParams.get("ReturnUrl");
        if (!returnUrl) {
            returnUrl = "/";
        }

        window.location.href = returnUrl;
    } catch (error) {
        if (error instanceof ProblemDetails) {
            console.error("Validation Failed:", error);
            alert(`Error: ${error.detail || "Invalid input data"}`);
        }

        else if (error instanceof ErrorDto) {
            console.error("Auth Failed:", error.code);
            alert(`Login Failed: ${error.message}`);
        }

        else if (ApiException.isApiException(error)) {
            console.error(`Server Error (${error.status}):`, error);
            alert("Something went wrong on our end.");
        }

        else {
            console.error("Unknown error:", error);
        }
    }

}
