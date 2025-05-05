import { type ClassValue, clsx } from "clsx";
import { twMerge } from "tailwind-merge";
import { toast, ToastOptions } from "react-hot-toast";

/**
 * Combines class names with Tailwind CSS classes safely
 */
export function cn(...inputs: ClassValue[]) {
    return twMerge(clsx(inputs));
}

/**
 * Format a date in a human-readable format
 */
export function formatDate(date: Date | string): string {
    const d = typeof date === "string" ? new Date(date) : date;
    return d.toLocaleDateString("en-US", {
        month: "long",
        day: "numeric",
        year: "numeric",
    });
}

/**
 * Truncate a string to a specified length and add ellipsis
 */
export function truncateText(text: string, maxLength: number): string {
    if (text.length <= maxLength) return text;
    return `${text.slice(0, maxLength)}...`;
}

/**
 * Get relative time (like "2 days ago") from a date
 */
export function getTimeAgo(dateStr: string | Date): string {
    const date = typeof dateStr === "string" ? new Date(dateStr) : dateStr;
    const now = new Date();
    const diffInSeconds = Math.floor((now.getTime() - date.getTime()) / 1000);
    const diffInMinutes = Math.floor(diffInSeconds / 60);
    const diffInHours = Math.floor(diffInMinutes / 60);
    const diffInDays = Math.floor(diffInHours / 24);

    if (diffInSeconds < 60) return "just now";
    if (diffInMinutes < 60)
        return `${diffInMinutes} minute${diffInMinutes > 1 ? "s" : ""} ago`;
    if (diffInHours < 24)
        return `${diffInHours} hour${diffInHours > 1 ? "s" : ""} ago`;
    return `${diffInDays} day${diffInDays > 1 ? "s" : ""} ago`;
}

/**
 * Custom toast with theme-aware styling
 */
type ToastTypes = "success" | "error" | "loading" | "custom";

interface CustomToastOptions extends Partial<ToastOptions> {
    icon?: string;
    type?: ToastTypes;
}

/**
 * Get the appropriate badge class for a workflow status
 */
export const getStatusBadgeClass = (status: string) => {
    switch (status?.toLowerCase()) {
        case "active":
            return "bg-green-500/10 text-green-600 dark:text-green-400";
        case "draft":
            return "bg-yellow-500/10 text-yellow-600 dark:text-yellow-400";
        case "archived":
            return "bg-gray-500/10 text-gray-600 dark:text-gray-400";
        default:
            return "bg-primary/10 text-primary-foreground";
    }
};

export const showToast = (message: string, options?: CustomToastOptions) => {
    const { icon, type = "custom", ...rest } = options || {};

    // Apply the appropriate toast type
    switch (type) {
        case "success":
            return toast.success(message, rest);
        case "error":
            return toast.error(message, rest);
        case "loading":
            return toast.loading(message, rest);
        case "custom":
        default:
            return toast(message, {
                icon,
                ...rest,
            });
    }
};
