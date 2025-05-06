"use client";

import { useState } from "react";
import { FlowEditor } from "./components/flow-editor";
import TaskLibrary from "./components/task-library";
import { FlowToolbar } from "./components/flow-toolbar";

export default function WorkflowEditor() {
    const [sidebarOpen, setSidebarOpen] = useState(true);

    return (
        <div className="flex flex-col h-full overflow-hidden">
            {/* Top toolbar */}
            <FlowToolbar
                name="Untitled Workflow"
                onRun={() => {}}
                sidebarOpen={sidebarOpen}
                setSidebarOpen={setSidebarOpen}
            />

            {/* Main content area with editor and sidebar */}
            <div className="flex flex-1 overflow-hidden">
                {/* Main editor canvas */}
                <div className="flex-1 bg-background/50 relative h-full p-4 bg-card">
                    <FlowEditor />
                </div>

                {/* Right sidebar */}
                <div
                    className={`border-l border-border bg-card transition-all duration-300 h-full ${
                        sidebarOpen ? "w-80" : "w-0"
                    } overflow-hidden`}
                >
                    <TaskLibrary
                        isOpen={sidebarOpen}
                    />
                </div>
            </div>
        </div>
    );
}
