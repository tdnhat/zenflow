import { DashboardSidebar } from "@/components/navigation";

export default function MainLayout({
    children,
}: {
    children: React.ReactNode;
}) {
    return <DashboardSidebar>{children}</DashboardSidebar>;
}
