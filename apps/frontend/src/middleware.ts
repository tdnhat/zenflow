import { NextRequest, NextResponse } from "next/server";
import { getToken } from "next-auth/jwt";

// Paths that are accessible for unauthenticated users
const publicPaths = ["/", "/signin", "/auth/error"];

export async function middleware(request: NextRequest) {
  const path = request.nextUrl.pathname;
  
  // Check if the path is public
  const isPublicPath = publicPaths.some(publicPath => 
    path === publicPath || path.startsWith(publicPath + "/")
  );

  // Allow access to public paths without authentication
  if (isPublicPath) {
    return NextResponse.next();
  }

  // Get the session token
  const token = await getToken({
    req: request,
    secret: process.env.NEXTAUTH_SECRET,
  });

  // Redirect to signin if there's no session
  if (!token) {
    const url = new URL("/signin", request.url);
    url.searchParams.set("callbackUrl", path);
    return NextResponse.redirect(url);
  }

  // Allow authenticated access
  return NextResponse.next();
}

// Configure which paths the middleware applies to
export const config = {
  matcher: [
    // Apply to all paths except static assets, api, and _next
    "/((?!_next/static|_next/image|favicon.ico|images|api/auth).*)",
  ],
}; 