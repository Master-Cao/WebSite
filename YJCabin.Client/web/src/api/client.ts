async function parse<T>(response: Response): Promise<T> {
  if (response.status === 204 || response.status === 202) {
    return undefined as T;
  }
  const text = await response.text();
  const data = text ? JSON.parse(text) : undefined;
  if (!response.ok) {
    throw new Error(data?.message ?? `Request failed (${response.status})`);
  }
  return data as T;
}

export async function api<T>(path: string, init: RequestInit = {}): Promise<T> {
  const headers = new Headers(init.headers);
  if (!(init.body instanceof FormData) && !headers.has("Content-Type")) {
    headers.set("Content-Type", "application/json");
  }
  const response = await fetch(path, { ...init, headers });
  return parse<T>(response);
}
