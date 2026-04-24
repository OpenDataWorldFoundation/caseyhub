import React, {
  createContext,
  useContext,
  useEffect,
  useState,
} from "react";

import { authApi } from "@/src/features/auth/auth.api";
import { authStorage } from "@/src/features/auth/auth.storage";
import {
  AuthSession,
  AuthStatus,
  AuthUser,
  LoginRequestDto,
  RegisterRequestDto,
} from "@/src/features/auth/types";

interface AuthContextValue {
  user: AuthUser | null;
  token: string | null;
  status: AuthStatus;
  isAuthenticated: boolean;
  login: (payload: LoginRequestDto) => Promise<void>;
  register: (payload: RegisterRequestDto) => Promise<void>;
  logout: () => Promise<void>;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

const isSessionExpired = (session: AuthSession) => {
  return new Date(session.expiresAtUtc).getTime() <= Date.now();
};

const mapSessionFromResponse = (
  response: Awaited<ReturnType<typeof authApi.login>>,
): AuthSession => ({
  user: {
    userId: response.userId,
    name: response.name,
    email: response.email,
  },
  token: response.token,
  expiresAtUtc: response.expiresAtUtc,
});

export const AuthProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const [session, setSession] = useState<AuthSession | null>(null);
  const [status, setStatus] = useState<AuthStatus>("loading");

  useEffect(() => {
    let isMounted = true;

    const restoreSession = async () => {
      try {
        const storedSession = await authStorage.getSession();

        if (!storedSession || isSessionExpired(storedSession)) {
          await authStorage.clearSession();

          if (isMounted) {
            setSession(null);
            setStatus("unauthenticated");
          }

          return;
        }

        if (isMounted) {
          setSession(storedSession);
          setStatus("authenticated");
        }
      } catch {
        await authStorage.clearSession();

        if (isMounted) {
          setSession(null);
          setStatus("unauthenticated");
        }
      }
    };

    restoreSession();

    return () => {
      isMounted = false;
    };
  }, []);

  const persistSession = async (
    sessionPromise: Promise<Awaited<ReturnType<typeof authApi.login>>>,
  ) => {
    const response = await sessionPromise;
    const nextSession = mapSessionFromResponse(response);

    await authStorage.saveSession(nextSession);
    setSession(nextSession);
    setStatus("authenticated");
  };

  const value: AuthContextValue = {
    user: session?.user ?? null,
    token: session?.token ?? null,
    status,
    isAuthenticated: status === "authenticated",
    login: async (payload) => {
      await persistSession(authApi.login(payload));
    },
    register: async (payload) => {
      await persistSession(authApi.register(payload));
    },
    logout: async () => {
      await authStorage.clearSession();
      setSession(null);
      setStatus("unauthenticated");
    },
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};

export const useAuth = () => {
  const context = useContext(AuthContext);

  if (!context) {
    throw new Error("useAuth must be used within an AuthProvider");
  }

  return context;
};
