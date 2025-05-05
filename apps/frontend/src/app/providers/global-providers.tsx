"use client";

import NextAuthProvider from "@/app/providers/next-auth-provider";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { useState } from "react";
import { ToastProvider } from "@/app/providers/toast-provider";
import { ThemeProvider } from "@/app/providers/theme-provider";

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
                    <ToastProvider />
                    {children}
                </NextAuthProvider>
            </ThemeProvider>
        </QueryClientProvider>
    );
}
