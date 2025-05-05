"use client";

import { useSearchParams } from "next/navigation";
import Link from "next/link";
import { Suspense } from "react";

function ErrorContent() {
  const searchParams = useSearchParams();
  const error = searchParams.get("error");

  let errorMessage = "An unknown error occurred during authentication.";
  
  if (error === "Configuration") {
    errorMessage = "There is a problem with the server configuration.";
  } else if (error === "AccessDenied") {
    errorMessage = "You do not have permission to sign in.";
  } else if (error === "Verification") {
    errorMessage = "The verification link has expired or has already been used.";
  }

  return (
    <div className="bg-white p-8 rounded-lg shadow-md max-w-md w-full">
      <h1 className="text-2xl font-bold mb-4 text-red-600">Authentication Error</h1>
      <p className="mb-6">{errorMessage}</p>
      <p className="text-sm text-gray-500 mb-4">Error code: {error || "unknown"}</p>
      <div className="flex justify-between">
        <Link 
          href="/"
          className="px-4 py-2 bg-gray-200 text-gray-800 rounded-md hover:bg-gray-300 transition"
        >
          Go Home
        </Link>
        <Link 
          href="/signin"
          className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 transition"
        >
          Try Again
        </Link>
      </div>
    </div>
  );
}

export default function AuthError() {
  return (
    <div className="flex min-h-screen flex-col items-center justify-center p-24">
      <Suspense fallback={
        <div className="bg-white p-8 rounded-lg shadow-md max-w-md w-full">
          <h1 className="text-2xl font-bold mb-4 text-blue-600">Loading...</h1>
          <p className="mb-6">Please wait while we process your request.</p>
        </div>
      }>
        <ErrorContent />
      </Suspense>
    </div>
  );
}