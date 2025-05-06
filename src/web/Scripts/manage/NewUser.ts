import { PostUser } from './api.js'
export async function NewUser(username: string, password: string, role: string | null, email: string | null) {
    // TODO: add field validations
    try {

        let result = await PostUser({
            uname: username,
            password: password,
            role: role,
            email: email
        });
    } catch (ex) {
        console.error(ex);
    }
}
