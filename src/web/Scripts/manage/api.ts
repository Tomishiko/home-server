import { post } from "jquery"

export interface User {
    uname: string,
    password: string,
    email: string | null,
    role: string | null
}
export async function PostUser(user: User) {
    let result =  await  fetch('/manager/adduser', {
        method: 'POST',
        body: JSON.stringify(user),
        headers: {
            "Content-Type": "application/json"
        }
    });
}
