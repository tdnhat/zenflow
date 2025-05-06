"use client";

import { useState, useEffect } from "react";
import { FlowEditor } from "./_components/flow-editor";
import TaskLibrary from "./_components/task-library";
import { FlowToolbar } from "./_components/flow-toolbar";
import { useWorkflow } from "../../_hooks/use-workflow";
import { useParams } from "next/navigation";
import { useWorkflowStore } from "@/store/workflow.store";
import { Loader2 } from "lucide-react";

export default function WorkflowEditor() {
    const { id } = useParams();
    const workflowId = id as string;
    
    // Fetch workflow data from API
    const { data: workflow, isLoading, isError, error } = useWorkflow(workflowId);
    
    // UI state
    const [sidebarOpen, setSidebarOpen] = useState(true);
    
    // Workflow store state
    const { initializeWorkflow, isInitialized, clearWorkflow } = useWorkflowStore();
    
    // Initialize workflow data in the store when it's loaded
    useEffect(() => {
        if (workflow && !isInitialized) {
            // Initialize the workflow with backend data
            initializeWorkflow(workflow);
        }
        
        // Clear workflow data when component unmounts
        return () => {
            clearWorkflow();
        };
    }, [workflow, initializeWorkflow, isInitialized, clearWorkflow]);

    // Loading states
    if (isLoading) {
        return (
            <div className="flex items-center justify-center h-full">
                <Loader2 className="w-8 h-8 animate-spin text-primary" />
                <span className="ml-2 text-lg">Loading workflow...</span>
            </div>
        );
    }

    // Error state
    if (isError) {
        return (
            <div className="flex flex-col items-center justify-center h-full text-destructive">
                <h2 className="text-xl font-semibold mb-2">Error loading workflow</h2>
                <p>{error?.message || "An unknown error occurred"}</p>
            </div>
        );
    }

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
                    {isInitialized ? (
                        <FlowEditor />
                    ) : (
                        <div className="flex items-center justify-center h-full">
                            <Loader2 className="w-6 h-6 animate-spin text-primary" />
                            <span className="ml-2">Initializing workflow editor...</span>
                        </div>
                    )}
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
