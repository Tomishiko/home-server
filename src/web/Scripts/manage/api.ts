import { loadMoreLogs } from "./loadMore";

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
export interface LoadMoreResponse {
    tableContent: string,
    cursor: string | null
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
export async function FetchMoreLogs(cursor: string | undefined): Promise<LoadMoreResponse | null> {

    if (cursor) {
        const response = await fetch(`/manager/logspartialtable?pagination=${cursor}`);
        if (response.ok) {
            const responseText = await response.text();
            const cursor = response.headers.get("X-Next-Cursor");
            return { cursor: cursor, tableContent: responseText };
        }
    }
    return null;
}
