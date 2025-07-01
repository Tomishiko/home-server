import { PostUser } from './api.js'
export async function NewUser(username: string, password: string, role: string | null, email: string | null): Promise<[boolean, string]> {
    // TODO: add field validations
    try {

        let result = await PostUser({
            uname: username,
            password: password,
            role: role.toLowerCase(),
            email: email
        });
        if (result.ok) {
            let msg = `User ${username} succesfully added to the system!`;
            return [true, msg];
        }
        else {
            let body = await result.text()
            let msg = `Unable to create user. ${body}`;
            return [false, msg];
        }
    } catch (ex) {
        console.error(ex);
    }
}
