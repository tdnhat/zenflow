"use client";
import { Workflow } from "lucide-react";

export const Logo = () => {
    return (
        <a
            href="/dashboard"
            className="relative z-20 flex items-center space-x-2 py-1 text-sm font-normal"
        >
            <Workflow className="h-5 w-5 ml-2 text-primary" />
            <span
                className="font-medium whitespace-pre text-sidebar-foreground"
            >
                ZenFlow
            </span>
        </a>
    );
};

export const LogoIcon = () => {
    return (
        <a
            href="/dashboard"
            className="relative z-20 flex items-center space-x-2 py-1 text-sm font-normal"
        >
            <Workflow className="h-5 w-5 ml-2 text-primary" />
        </a>
    );
}; 