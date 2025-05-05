"use client";
import { usePathname } from "next/navigation";
import { motion, AnimatePresence } from "motion/react";
import { X } from "lucide-react";
import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";
import { NavLink } from "./nav-link";

interface MobileSidebarContentProps {
    isOpen: boolean;
    onClose: () => void;
    links: NavLink[];
}

export function MobileSidebarContent({
    isOpen,
    onClose,
    links,
}: MobileSidebarContentProps) {
    // Get current path to detect active link
    const pathname = usePathname();

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

                        <div className="pt-8 flex flex-col h-full">
                            <div className="flex flex-col gap-2">
                                {links.map((link, idx) => {
                                    const isActive = pathname === link.href;
                                    return (
                                        <a
                                            key={idx}
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
                                })}
                            </div>
                        </div>
                    </motion.div>
                )}
            </AnimatePresence>
        </div>
    );
} 