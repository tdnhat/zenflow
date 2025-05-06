"use client";

import { useTheme } from "next-themes";
import { Toaster, ToasterProps } from "react-hot-toast";
import { useEffect, useState } from "react";

export function ToastProvider({ ...props }: ToasterProps) {
    const { theme } = useTheme();
    const [mounted, setMounted] = useState(false);

    // Prevent hydration mismatch by only rendering after mount
    useEffect(() => {
        setMounted(true);
    }, []);

    if (!mounted) {
        return null;
    }

    const isDark = theme === "dark";

    return (
        <Toaster
            position="top-right"
            toastOptions={{
                // Default options for all toasts
                duration: 5000,
                style: {
                    background: isDark ? "var(--card)" : "var(--background)",
                    color: isDark ? "var(--card-foreground)" : "var(--foreground)",
                    border: "1px solid var(--border)",
                    borderRadius: "0.5rem",
                    fontSize: "0.875rem",
                    boxShadow: "var(--shadow-sm)",
                },
                // Default options for specific toast types
                success: {
                    iconTheme: {
                        primary: "var(--primary)",
                        secondary: "var(--primary-foreground)",
                    },
                },
                error: {
                    iconTheme: {
                        primary: "var(--destructive)",
                        secondary: "white",
                    },
                },
                loading: {
                    iconTheme: {
                        primary: "var(--muted-foreground)",
                        secondary: "white",
                    },
                },
            }}
            {...props}
        />
    );
} 