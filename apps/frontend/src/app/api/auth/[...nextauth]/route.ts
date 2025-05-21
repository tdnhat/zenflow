import NextAuth from "next-auth";
import KeycloakProvider from "next-auth/providers/keycloak";
import { JWT } from "next-auth/jwt";

// Extend the Session type to include our custom properties
declare module "next-auth" {
  interface Session {
    accessToken?: string;
    refreshToken?: string;
    idToken?: string;
    error?: string;
    tokenExpiry?: number;
  }
}

// Extend the JWT type
declare module "next-auth/jwt" {
  interface JWT {
    accessToken?: string;
    refreshToken?: string;
    idToken?: string;
    expiresAt?: number;
    error?: string;
  }
}

/**
 * Refreshes an OAuth access token using a refresh token
 */
async function refreshAccessToken(token: JWT): Promise<JWT> {
  try {
    const keycloakIssuer = `${process.env.NEXT_PUBLIC_KEYCLOAK_URL || "http://localhost:8080"}/realms/${process.env.NEXT_PUBLIC_KEYCLOAK_REALM || "zenflow"}`;
    
    // Get the refresh token endpoint URL
    const refreshUrl = `${keycloakIssuer}/protocol/openid-connect/token`;
    
    // Make the refresh token request
    const response = await fetch(refreshUrl, {
      headers: { "Content-Type": "application/x-www-form-urlencoded" },
      method: "POST",
      body: new URLSearchParams({
        client_id: process.env.NEXT_PUBLIC_KEYCLOAK_CLIENT_ID || "zenflow-frontend",
        client_secret: process.env.KEYCLOAK_CLIENT_SECRET || "your-client-secret",
        grant_type: "refresh_token",
        refresh_token: token.refreshToken as string,
      }),
    });

    const refreshedTokens = await response.json();

    if (!response.ok) {
      throw refreshedTokens;
    }

    return {
      ...token,
      accessToken: refreshedTokens.access_token,
      refreshToken: refreshedTokens.refresh_token ?? token.refreshToken, // Fall back to old refresh token if not provided
      idToken: refreshedTokens.id_token,
      expiresAt: Date.now() + (refreshedTokens.expires_in * 1000),
    };
  } catch (error) {
    console.error("RefreshAccessTokenError", error);
    
    return {
      ...token,
      error: "RefreshAccessTokenError",
    };
  }
}

// Configure authentication providers
const handler = NextAuth({
  providers: [
    KeycloakProvider({
      clientId: process.env.NEXT_PUBLIC_KEYCLOAK_CLIENT_ID || "zenflow-frontend",
      clientSecret: process.env.KEYCLOAK_CLIENT_SECRET || "your-client-secret",
      issuer: `${process.env.NEXT_PUBLIC_KEYCLOAK_URL || "http://localhost:8080"}/realms/${process.env.NEXT_PUBLIC_KEYCLOAK_REALM || "zenflow"}`,
    }),
  ],
  callbacks: {
    async jwt({ token, account }) {
      // Initial sign in
      if (account) {
        return {
          accessToken: account.access_token,
          refreshToken: account.refresh_token,
          idToken: account.id_token,
          expiresAt: (account.expires_at || 0) * 1000,
        };
      }

      // Return previous token if the access token has not expired yet
      // Add a buffer of 60 seconds to avoid edge cases
      if (token.expiresAt && Date.now() < token.expiresAt - 60000) {
        return token;
      }
      
      // Access token has expired, try to refresh it
      return refreshAccessToken(token);
    },
    async session({ session, token }) {
      // Send properties to the client
      session.accessToken = token.accessToken;
      session.refreshToken = token.refreshToken;
      session.idToken = token.idToken;
      session.error = token.error;
      session.tokenExpiry = token.expiresAt;
      return session;
    },
  },
  // Configure your session strategy
  session: {
    strategy: "jwt",
    // Decrease maxAge to force frequent refreshes for testing (optional)
    // maxAge: 60 * 60, // 1 hour
  },
  pages: {
    signIn: "/signin",
    error: "/error",
  },
});

export { handler as GET, handler as POST };