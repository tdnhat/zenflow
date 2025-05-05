"use client";
import React, { useState } from "react";
import {
    IconArrowLeft,
    IconBrandTabler,
    IconSettings,
    IconUserBolt,
    IconMenu2,
} from "@tabler/icons-react";
import { cn } from "@/lib/utils";
import { User } from "lucide-react";
import { Header } from "@/components/layouts/header";
import { Logo } from "./logo";
import { DesktopNavLink, NavLink } from "./nav-link";
import { MobileSidebarContent } from "./mobile-sidebar";
import { Button } from "@/components/ui/button";

export function DashboardSidebar({ children }: { children: React.ReactNode }) {
    const links: NavLink[] = [
        {
            label: "Dashboard",
            href: "/dashboard",
            icon: <IconBrandTabler className="h-5 w-5 shrink-0" />,
        },
        {
            label: "Workflows",
            href: "/workflows",
            icon: <IconUserBolt className="h-5 w-5 shrink-0" />,
        },
        {
            label: "Reports",
            href: "/reports",
            icon: <IconSettings className="h-5 w-5 shrink-0" />,
        },
        {
            label: "Settings",
            href: "/settings",
            icon: <IconArrowLeft className="h-5 w-5 shrink-0" />,
        },
    ];
    const [sidebarOpen, setSidebarOpen] = useState(true);

    const toggleSidebar = () => {
        setSidebarOpen((prev) => !prev);
    };

    return (
        <div
            className={cn(
                "mx-auto flex w-full flex-1 flex-col overflow-hidden bg-background md:flex-row",
                "h-screen"
            )}
        >
            {/* Desktop sidebar with toggle button */}
            <div
                className={cn(
                    "h-full hidden md:flex md:flex-col bg-sidebar border-r border-sidebar-border transition-all duration-300 ease-in-out",
                    sidebarOpen ? "w-[240px]" : "w-[68px]"
                )}
            >
                <div className="px-4 py-4 flex-1 flex flex-col justify-between gap-10">
                    <div className="flex flex-1 flex-col overflow-x-hidden overflow-y-auto">
                        {/* Toggle button replacing logo */}
                        <div className="flex items-center mb-6">
                            <Button
                                variant="ghost"
                                size="icon"
                                onClick={toggleSidebar}
                                className="h-8 w-8 p-0 text-sidebar-foreground hover:text-sidebar-primary dark:text-sidebar-foreground dark:hover:text-sidebar-primary transition-colors"
                                aria-label={
                                    sidebarOpen
                                        ? "Collapse sidebar"
                                        : "Expand sidebar"
                                }
                                tabIndex={0}
                            >
                                <IconMenu2 className="h-5 w-5" />
                                <span className="sr-only">
                                    {sidebarOpen
                                        ? "Collapse sidebar"
                                        : "Expand sidebar"}
                                </span>
                            </Button>
                            {sidebarOpen && <Logo />}
                        </div>

                        {/* Nav links */}
                        <div className="mt-8 flex flex-col gap-2">
                            {links.map((link, idx) => (
                                <DesktopNavLink
                                    key={idx}
                                    link={link}
                                    isExpanded={sidebarOpen}
                                />
                            ))}
                        </div>
                    </div>

                    {/* User link */}
                    <div>
                        <DesktopNavLink
                            link={{
                                label: "ZenFlow User",
                                href: "#",
                                icon: (
                                    <User className="h-5 w-5 shrink-0 rounded-full" />
                                ),
                            }}
                            isExpanded={sidebarOpen}
                        />
                    </div>
                </div>
            </div>

            {/* Mobile menu overlay */}
            <MobileSidebarContent
                isOpen={sidebarOpen}
                onClose={() => setSidebarOpen(false)}
                links={links}
            />

            <div className="flex flex-1">
                <div className="flex h-full w-full flex-1 flex-col gap-2 bg-card">
                    <Header onOpenSidebar={() => setSidebarOpen(true)} />
                    <main className="flex-1 overflow-y-auto p-4 md:p-6 text-card-foreground">
                        {children}
                    </main>
                </div>
            </div>
        </div>
    );
}
