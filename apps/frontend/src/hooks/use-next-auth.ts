"use client";

import { useSession, signIn } from "next-auth/react";

export function useNextAuth() {
    const { data: session, status } = useSession();

    const performFullLogout = () => {
        if (!session?.idToken) {
            console.error("No ID token found. Cannot perform full logout.");
            return;
        }

        const keycloakUrl = process.env.NEXT_PUBLIC_KEYCLOAK_URL;
        const realm = process.env.NEXT_PUBLIC_KEYCLOAK_REALM;

        const logoutUrl = `${keycloakUrl}/realms/${realm}/protocol/openid-connect/logout`;
        const redirectUri = encodeURIComponent(
            `${window.location.origin}/signin`
        );

        const fullLogoutUrl = `${logoutUrl}?post_logout_redirect_uri=${redirectUri}&id_token_hint=${session.idToken}`;

        window.location.href = fullLogoutUrl;
    };

    return {
        isAuthenticated: status === "authenticated",
        isLoading: status === "loading",
        user: session?.user,
        accessToken: session?.accessToken,
        login: () => signIn("keycloak"),
        logout: performFullLogout,
    };
}
