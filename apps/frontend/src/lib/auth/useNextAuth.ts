"use client";

import { useSession, signIn, signOut } from "next-auth/react";

export function useNextAuth() {
  const { data: session, status } = useSession();

  return {
    isAuthenticated: status === "authenticated",
    isLoading: status === "loading",
    user: session?.user,
    accessToken: session?.accessToken,
    login: () => signIn("keycloak"),
    logout: () => signOut(),
  };
} 