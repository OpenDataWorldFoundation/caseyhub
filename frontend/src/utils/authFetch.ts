import { BACKEND_BASE_URL } from "../constants/api";

export const authFetch = async (endpoint: string, token: string | null, options: RequestInit = {}): Promise<Response> => {
    const headers: Record<string, string> = {"Content-Type":"application/json",...(options.headers as Record<string, string>)};

    if (token){
        headers["Authorization"] = `Bearer ${token}`;
    }

    const response = await fetch (`${BACKEND_BASE_URL}${endpoint}`, {
        ...options,
        headers
        }
    );
    return response;
}