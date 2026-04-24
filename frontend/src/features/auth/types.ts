export interface LoginRequestDto {
  email: string;
  password: string;
}

export interface RegisterRequestDto {
  name: string;
  email: string;
  password: string;
}

export interface AuthResponseDto {
  userId: string;
  name: string;
  email: string;
  token: string;
  expiresAtUtc: string;
}

export interface AuthUser {
  userId: string;
  name: string;
  email: string;
}

export interface AuthSession {
  user: AuthUser;
  token: string;
  expiresAtUtc: string;
}

export type AuthStatus = "loading" | "authenticated" | "unauthenticated";
