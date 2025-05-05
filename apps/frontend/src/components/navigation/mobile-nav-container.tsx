import { usePathname } from "next/navigation";
import { cn } from "@/lib/utils";
import { motion, AnimatePresence } from "framer-motion";
import { Button } from "@/components/ui/button";
import { X } from "lucide-react";

interface NavLink {
    label: string;
    href: string;
    icon: React.ReactNode;
}

export function MobileSidebarContent({
    isOpen,
    onClose,
    links,
    userLink,
}: {
    isOpen: boolean;
    onClose: () => void;
    links: NavLink[];
    userLink: NavLink;
}) {
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
                <span className="text-base">{link.label}</span>
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
