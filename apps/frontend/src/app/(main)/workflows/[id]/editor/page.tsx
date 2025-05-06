"use client";

import { useState, useEffect } from "react";
import { FlowEditor } from "./_components/flow-editor";
import TaskLibrary from "./_components/task-library";
import { FlowToolbar } from "./_components/flow-toolbar";
import { useWorkflow } from "../../_hooks/use-workflow";
import { useParams } from "next/navigation";
import { useWorkflowStore } from "@/store/workflow.store";

export default function WorkflowEditor() {
    const { id } = useParams();
    const { data: workflow, isLoading, isError } = useWorkflow(id as string);
    const [sidebarOpen, setSidebarOpen] = useState(true);
    
    // Get the initializeWorkflow function from the store
    const { initializeWorkflow, isInitialized } = useWorkflowStore();
    
    // Initialize workflow data in the store when it's loaded
    useEffect(() => {
        if (workflow && !isInitialized) {
            initializeWorkflow(workflow);
        }
    }, [workflow, initializeWorkflow, isInitialized]);

    if (isLoading) return <div>Loading...</div>;
    if (isError) return <div>Error loading workflow</div>;

    return (
        <div className="flex h-full overflow-hidden">
            {/* Main content area with toolbar and editor */}
            <div className="flex flex-col flex-1 overflow-hidden">
                {/* Top toolbar */}
                <FlowToolbar
                    name={workflow?.name || "Untitled Workflow"}
                    onRun={() => {}}
                    sidebarOpen={sidebarOpen}
                    setSidebarOpen={setSidebarOpen}
                />

                {/* Main editor canvas */}
                <div className="flex-1 relative h-full">
                    <FlowEditor />
                </div>
            </div>

            {/* Right sidebar outside main content */}
            <div
                className={`border-l transition-all duration-300 h-full ${
                    sidebarOpen ? "w-80" : "w-0"
                } overflow-hidden`}
            >
                <TaskLibrary isOpen={sidebarOpen} />
            </div>
        </div>
    );
}
