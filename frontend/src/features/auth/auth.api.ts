import { buildApiUrl } from "@/src/lib/api";

import {
  AuthResponseDto,
  LoginRequestDto,
  RegisterRequestDto,
} from "./types";

interface ApiErrorResponse {
  message?: string;
}

export class AuthApiError extends Error {
  status: number;

  constructor(message: string, status: number) {
    super(message);
    this.name = "AuthApiError";
    this.status = status;
  }
}

const getErrorMessage = async (response: Response) => {
  try {
    const payload = (await response.json()) as ApiErrorResponse;
    return payload.message || "Something went wrong. Please try again.";
  } catch {
    return "Something went wrong. Please try again.";
  }
};

const postAuthRequest = async <TPayload extends object>(
  path: string,
  payload: TPayload,
) => {
  const response = await fetch(buildApiUrl(path), {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(payload),
  });

  console.log("response from backend: " + response);

  if (!response.ok) {
    throw new AuthApiError(await getErrorMessage(response), response.status);
  }
  

  return (await response.json()) as AuthResponseDto;
};

export const authApi = {
  login(payload: LoginRequestDto) {
    return postAuthRequest("/api/auth/login", payload);
  },
  register(payload: RegisterRequestDto) {
    return postAuthRequest("/api/auth/register", payload);
  },
};
