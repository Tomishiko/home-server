export interface User {
    Username: string,
    Password: string,
    Email: string | null,
    Role: number | null
}
export const enum UserRole {
    User = 1,
    Manager = 2,
}
export const RoleNames: Record<UserRole, string> = {
    [UserRole.User]: "Standard User",
    [UserRole.Manager]: "Manager",
};
export async function PostUser(user: User) {
    return await fetch('/api/user', {
        method: 'POST',
        body: JSON.stringify(user),
        headers: {
            "Content-Type": "application/json"
        }
    });

}
export async function FetchMoreLogs(cursor: string) {
    return await fetch(`/manager/logspartialtable?lastitem=${cursor}`)
        .then(response => response.text())
}
