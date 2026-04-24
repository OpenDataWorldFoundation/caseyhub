import * as SecureStore from "expo-secure-store";

import { AuthSession } from "./types";

const AUTH_SESSION_KEY = "caseyhub.auth.session";

export const authStorage = {
  async getSession() {
    const storedSession = await SecureStore.getItemAsync(AUTH_SESSION_KEY);

    if (!storedSession) {
      return null;
    }

    try {
      return JSON.parse(storedSession) as AuthSession;
    } catch {
      await SecureStore.deleteItemAsync(AUTH_SESSION_KEY);
      return null;
    }
  },
  async saveSession(session: AuthSession) {
    await SecureStore.setItemAsync(
      AUTH_SESSION_KEY,
      JSON.stringify(session),
    );
  },
  async clearSession() {
    await SecureStore.deleteItemAsync(AUTH_SESSION_KEY);
  },
};
