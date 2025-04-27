import NextAuth from "next-auth";
import KeycloakProvider from "next-auth/providers/keycloak";

// Extend the Session type to include our custom properties
declare module "next-auth" {
  interface Session {
    accessToken?: string;
    refreshToken?: string;
    idToken?: string;
    error?: string;
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
      // Persist the OAuth access_token and refresh_token to the token right after signin
      if (account) {
        token.accessToken = account.access_token;
        token.refreshToken = account.refresh_token;
        token.idToken = account.id_token;
        token.expiresAt = (account.expires_at || 0) * 1000;
      }
      return token;
    },
    async session({ session, token }) {
      // Send properties to the client
      session.accessToken = token.accessToken;
      session.refreshToken = token.refreshToken;
      session.idToken = token.idToken;
      session.error = token.error;
      return session;
    },
  },
  // Configure your session strategy
  session: {
    strategy: "jwt",
  },
  pages: {
    signIn: "/auth/signin",
    error: "/auth/error",
  },
});

export { handler as GET, handler as POST }; 