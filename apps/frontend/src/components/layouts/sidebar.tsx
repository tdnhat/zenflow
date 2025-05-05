"use client";
import React, { useState } from "react";
import { usePathname } from "next/navigation";
import {
    IconArrowLeft,
    IconBrandTabler,
    IconSettings,
    IconUserBolt,
} from "@tabler/icons-react";
import { motion, AnimatePresence } from "motion/react";
import { cn } from "@/lib/utils";
import { User, X } from "lucide-react";
import { Header } from "./header";
import { Button } from "@/components/ui/button";

interface NavLink {
    label: string;
    href: string;
    icon: React.ReactNode;
}

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

    // Create a user profile link
    const userLink: NavLink = {
        label: "ZenFlow User",
        href: "/profile",
        icon: <User className="h-5 w-5 shrink-0" />,
    };

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
                <div className="px-4 py-4 flex-1 flex flex-col justify-between">
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
                    <div className="mt-8">
                        <DesktopNavLink
                            link={userLink}
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
                userLink={userLink}
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

interface DesktopNavLinkProps {
    link: NavLink;
    isExpanded: boolean;
}

function DesktopNavLink({ link, isExpanded }: DesktopNavLinkProps) {
    // Get current path to detect active link
    const pathname = usePathname();
    const isActive = pathname === link.href;

    return (
        <a
            href={link.href}
            className={cn(
                "flex items-center justify-start gap-2 group py-2 px-2 rounded-md transition-all",
                isActive
                    ? "bg-primary/10 text-primary"
                    : "hover:text-sidebar-primary hover:bg-sidebar-accent/10"
            )}
        >
            <div
                className={cn(
                    "flex items-center justify-center transition-all",
                    isActive
                        ? "text-primary scale-110"
                        : "text-sidebar-foreground group-hover:text-sidebar-primary group-hover:scale-105"
                )}
            >
                {link.icon}
            </div>

            <motion.span
                animate={{
                    display: isExpanded ? "inline-block" : "none",
                    opacity: isExpanded ? 1 : 0,
                }}
                className={cn(
                    "text-sm group-hover:translate-x-1 transition duration-150 whitespace-pre inline-block !p-0 !m-0",
                    isActive ? "font-medium" : "font-normal"
                )}
            >
                {link.label}
            </motion.span>
        </a>
    );
}

interface MobileSidebarContentProps {
    isOpen: boolean;
    onClose: () => void;
    links: NavLink[];
    userLink: NavLink;
}

function MobileSidebarContent({
    isOpen,
    onClose,
    links,
    userLink,
}: MobileSidebarContentProps) {
    // Get current path to detect active link
    const pathname = usePathname();

    // Render a mobile nav link
    const renderNavLink = (link: NavLink) => {
        const isActive = pathname === link.href;
        return (
            <a
                key={link.href}
                href={link.href}
                className={cn(
                    "flex items-center gap-3 py-2 px-3 rounded-md transition-all",
                    isActive
                        ? "bg-primary/10 text-primary font-medium"
                        : "text-sidebar-foreground hover:text-sidebar-primary hover:bg-sidebar-accent/10"
                )}
            >
                <div
                    className={cn(
                        "flex items-center justify-center transition-all",
                        isActive
                            ? "text-primary scale-110"
                            : "text-sidebar-foreground hover:text-sidebar-primary hover:scale-105"
                    )}
                >
                    {link.icon}
                </div>
                <span className="text-base">
                    {link.label}
                </span>
            </a>
        );
    };

    return (
        <div className="fixed inset-0 z-[91] pointer-events-none md:hidden">
            <AnimatePresence>
                {isOpen && (
                    <motion.div
                        initial={{ x: "-100%", opacity: 0 }}
                        animate={{ x: 0, opacity: 1 }}
                        exit={{ x: "-100%", opacity: 0 }}
                        transition={{
                            duration: 0.3,
                            ease: "easeInOut",
                        }}
                        className="fixed h-full w-full inset-0 bg-sidebar pt-16 px-6 z-[90] flex flex-col pointer-events-auto"
                    >
                        <Button
                            variant="ghost"
                            size="icon"
                            className="absolute right-4 top-4 text-sidebar-foreground"
                            onClick={onClose}
                        >
                            <X className="h-5 w-5" />
                            <span className="sr-only">Close menu</span>
                        </Button>

                        <div className="pt-8 flex flex-col h-full justify-between">
                            <div className="flex flex-col gap-2">
                                {links.map(renderNavLink)}
                            </div>
                            
                            <div className="mb-8 border-t border-sidebar-border pt-4">
                                {renderNavLink(userLink)}
                            </div>
                        </div>
                    </motion.div>
                )}
            </AnimatePresence>
        </div>
    );
}

export const Logo = () => {
    return (
        <a
            href="/dashboard"
            className="relative z-20 flex items-center space-x-2 py-1 text-sm font-normal"
        >
            <IconBrandTabler className="h-5 w-5 ml-2 shrink-0" />
            <motion.span
                initial={{ opacity: 0 }}
                animate={{ opacity: 1 }}
                className="font-medium whitespace-pre text-sidebar-foreground"
            >
                ZenFlow
            </motion.span>
        </a>
    );
};

export const LogoIcon = () => {
    return (
        <a
            href="/dashboard"
            className="relative z-20 flex items-center py-1 text-sm font-normal"
        >
            <IconBrandTabler className="h-5 w-5 ml-2 shrink-0" />
        </a>
    );
};
