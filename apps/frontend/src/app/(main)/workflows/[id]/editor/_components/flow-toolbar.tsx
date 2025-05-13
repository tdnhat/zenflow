import { Button } from "@/components/ui/button";
import { PanelRightClose, PanelRightOpen, Play } from "lucide-react";

interface FlowToolbarProps {
    name: string;
    onRun: () => void;
    sidebarOpen: boolean;
    setSidebarOpen: (open: boolean) => void;
}

export const FlowToolbar = ({
    name,
    onRun,
    sidebarOpen,
    setSidebarOpen,
}: FlowToolbarProps) => {
    return (
        <div className="h-16 p-4 border-b flex items-center justify-between">
            <div>
                <h2 className="text-xl font-semibold">
                    {name || "Untitled Workflow"}
                </h2>
            </div>
            {/* Sidebar toggle button */}
            <div className="flex items-center gap-2">
                <Button size="sm" onClick={onRun}>
                    <Play className="h-4 w-4 mr-2" />
                    Run
                </Button>
                <Button
                    variant="outline"
                    size="sm"
                    onClick={() => setSidebarOpen(!sidebarOpen)}
                >
                    {sidebarOpen ? (
                        <PanelRightClose className="h-4 w-4" />
                    ) : (
                        <PanelRightOpen className="h-4 w-4" />
                    )}
                </Button>
            </div>
        </div>
    );
};
