/**
 * Retrieves a cookie value by name.
 * @param name The key of the cookie to retrieve.
 * @returns The string value of the cookie, or undefined if not found.
 */
export function getCookie(name: string): string | undefined {
  const value: string = `; ${document.cookie}`;
  const parts: string[] = value.split(`; ${name}=`);

  if (parts.length === 2) {
    return parts.pop()?.split(';').shift();
  }

  return undefined;
}
