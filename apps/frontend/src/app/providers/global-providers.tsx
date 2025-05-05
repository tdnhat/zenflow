"use client";

import { ThemeProvider } from "@/components/ui/theme-provider";
import NextAuthProvider from "@/app/providers/next-auth-provider";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { Toaster } from "react-hot-toast";
import { useState } from "react";

export default function GlobalProviders({
    children,
}: {
    children: React.ReactNode;
}) {
    const [queryClient] = useState(
        () =>
            new QueryClient({
                defaultOptions: {
                    queries: {
                        staleTime: 60 * 1000, // 1 minute
                        retry: 1,
                        refetchOnWindowFocus: false,
                    },
                },
            })
    );

    return (
        <QueryClientProvider client={queryClient}>
            <ThemeProvider
                attribute="class"
                defaultTheme="dark"
                enableSystem
                disableTransitionOnChange
            >
                <NextAuthProvider>
                    <Toaster />
                    {children}
                </NextAuthProvider>
            </ThemeProvider>
        </QueryClientProvider>
    );
}
