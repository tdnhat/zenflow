"use client";
import React, { useState } from "react";
import {
    IconArrowLeft,
    IconBrandTabler,
    IconSettings,
    IconUserBolt,
} from "@tabler/icons-react";
import { motion } from "motion/react";
import { cn } from "@/lib/utils";
import { User } from "lucide-react";
import { Header } from "@/components/global/header";
import { Logo, LogoIcon } from "./logo";
import { DesktopNavLink, NavLink } from "./nav-link";
import { MobileSidebarContent } from "./mobile-sidebar";

export function DashboardSidebar({ children }: { children: React.ReactNode }) {
    const links: NavLink[] = [
        {
            label: "Dashboard",
            href: "/dashboard",
            icon: <IconBrandTabler className="h-5 w-5 shrink-0" />,
        },
        {
            label: "Workflows",
            href: "/dashboard/workflows",
            icon: <IconUserBolt className="h-5 w-5 shrink-0" />,
        },
        {
            label: "Reports",
            href: "/dashboard/reports",
            icon: <IconSettings className="h-5 w-5 shrink-0" />,
        },
        {
            label: "Settings",
            href: "/dashboard/settings",
            icon: <IconArrowLeft className="h-5 w-5 shrink-0" />,
        },
    ];
    const [sidebarOpen, setSidebarOpen] = useState(false);

    return (
        <div
            className={cn(
                "mx-auto flex w-full flex-1 flex-col overflow-hidden bg-background md:flex-row",
                "h-screen"
            )}
        >
            {/* Desktop sidebar with hover effect */}
            <motion.div
                className="h-full hidden md:flex md:flex-col bg-sidebar border-r shrink-0"
                animate={{
                    width: sidebarOpen ? "240px" : "68px",
                }}
                onMouseEnter={() => setSidebarOpen(true)}
                onMouseLeave={() => setSidebarOpen(false)}
            >
                <div className="px-4 py-4 flex-1 flex flex-col justify-between gap-10">
                    <div className="flex flex-1 flex-col overflow-x-hidden overflow-y-auto">
                        {/* Logo */}
                        {sidebarOpen ? <Logo /> : <LogoIcon />}

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
            </motion.div>

            {/* Mobile menu overlay */}
            <MobileSidebarContent
                isOpen={sidebarOpen}
                onClose={() => setSidebarOpen(false)}
                links={links}
            />

            <div className="flex flex-1">
                <div className="flex h-full w-full flex-1 flex-col gap-2 bg-card p-2 md:p-10">
                    <Header onOpenSidebar={() => setSidebarOpen(true)} />
                    <main className="flex-1 overflow-y-auto p-4 md:p-6 text-card-foreground">
                        {children}
                    </main>
                </div>
            </div>
        </div>
    );
} 