import { DashboardSidebar } from "@/components/navigation";

export default function DashboardLayout({ children }: { children: React.ReactNode }) {
    return (
        <DashboardSidebar>{children}</DashboardSidebar>
    );
}
