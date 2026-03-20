export class ClientBase {

    protected transformOptions(options: RequestInit): Promise<RequestInit> {
        const element = document.getElementsByName("__RequestVerificationToken")[0] as HTMLInputElement;
        const token = element ? element.value : "";
        if (!token) return Promise.resolve(options);

        if (options.body instanceof FormData) {
            options.body.append('__RequestVerificationToken', token);

        }
        else {
            options.headers = {
                ...options.headers,
                "X-XSRF-TOKEN": token
            };
        }

        return Promise.resolve(options);
    }
}
