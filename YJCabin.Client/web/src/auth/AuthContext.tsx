import { createContext, useContext, useMemo, useState, type ReactNode } from "react";
import { api, clearSession, getAccessToken, setSession } from "../api/client";
import type { AuthResponse } from "../api/types";

type AuthState = {
  isAuthenticated: boolean;
  userName?: string;
  login: (userName: string, password: string) => Promise<void>;
  logout: () => void;
};

const AuthContext = createContext<AuthState | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [userName, setUserName] = useState<string | undefined>(() => (getAccessToken() ? "admin" : undefined));

  const value = useMemo<AuthState>(
    () => ({
      isAuthenticated: Boolean(getAccessToken()),
      userName,
      login: async (name, password) => {
        const auth = await api<AuthResponse>("/api/auth/login", {
          method: "POST",
          body: JSON.stringify({ userName: name, password })
        });
        setSession(auth);
        setUserName(auth.userName);
      },
      logout: () => {
        clearSession();
        setUserName(undefined);
      }
    }),
    [userName]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) {
    throw new Error("useAuth must be used within AuthProvider");
  }
  return ctx;
}
