"use client";

import { useState } from "react";
import { Button } from "@/components/ui/button";
import {
    ChevronRight,
    ZoomIn,
    ZoomOut,
    Grid3X3,
    Play,
} from "lucide-react";
import { FlowEditor } from "./components/flow-editor";
import TaskLibrary from "./components/task-library";

export default function WorkflowEditor() {
    const [sidebarOpen, setSidebarOpen] = useState(true);

    return (
        <div className="flex flex-col h-[calc(100vh-64px)]">
            {/* Top toolbar */}
            <div className="bg-card border-b border-border p-2 flex items-center justify-between">
                <div className="flex items-center gap-2">
                    <Button variant="outline" size="icon" className="h-8 w-8">
                        <ZoomIn className="h-4 w-4" />
                    </Button>

                    <Button variant="outline" size="icon" className="h-8 w-8">
                        <ZoomOut className="h-4 w-4" />
                    </Button>

                    <Button variant="outline" size="icon" className="h-8 w-8">
                        <Grid3X3 className="h-4 w-4" />
                    </Button>
                </div>

                <div>
                    <h2 className="text-xl font-semibold">Workflow Editor</h2>
                </div>

                <div>
                    <Button variant="outline" size="sm">
                        <Play className="h-4 w-4 mr-2" />
                        Run
                    </Button>
                </div>
            </div>

            {/* Main content area with editor and sidebar */}
            <div className="flex flex-1 overflow-hidden">
                {/* Main editor canvas */}
                <div className="flex-1 bg-background/50 relative">
                    <FlowEditor  />
                </div>

                {/* Right sidebar */}
                <div
                    className={`border-l border-border bg-card transition-all duration-300 ${
                        sidebarOpen ? "w-80" : "w-0"
                    } overflow-hidden`}
                >
                    <TaskLibrary
                        isOpen={sidebarOpen}
                        onClose={() => setSidebarOpen(false)}
                    />
                </div>

                {/* Sidebar toggle button (shown when sidebar is closed) */}
                {!sidebarOpen && (
                    <Button
                        variant="outline"
                        size="icon"
                        className="absolute right-4 top-4 z-10"
                        onClick={() => setSidebarOpen(true)}
                    >
                        <ChevronRight className="h-4 w-4 rotate-180" />
                    </Button>
                )}
            </div>
        </div>
    );
}
