import { usePathname } from "next/navigation";
import { cn } from "@/lib/utils";
import { motion } from "framer-motion";

interface NavLink {
    label: string;
    href: string;
    icon: React.ReactNode;
}


export function DesktopNavLink({ link, isExpanded }: { link: NavLink, isExpanded: boolean }) {
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