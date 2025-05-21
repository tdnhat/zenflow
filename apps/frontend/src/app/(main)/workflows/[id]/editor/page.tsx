"use client";

import { useState, useEffect, useCallback } from "react";
import { FlowEditor } from "./_components/flow-editor";
import TaskLibrary from "./_components/task-library";
import { FlowToolbar } from "./_components/flow-toolbar";
import { useWorkflow } from "../../_hooks/use-workflow";
import { useParams } from "next/navigation";
import { useWorkflowStore } from "@/store/workflow.store";
import { Loader2 } from "lucide-react";
import WorkflowError from "../../_components/workflow-error";

export default function WorkflowEditor() {
    const { id } = useParams();
    const workflowId = id as string;

    // Fetch workflow data from API
    const {
        data: workflow,
        isLoading,
        isError,
        refetch,
    } = useWorkflow(workflowId);

    // UI state
    const [sidebarOpen, setSidebarOpen] = useState(true);

    // Workflow store state
    const { initializeWorkflow, isInitialized, clearWorkflow } =
        useWorkflowStore();

    // Initialize workflow data in the store when it's loaded
    useEffect(() => {
        if (workflow && !isInitialized) {
            // Initialize the workflow with backend data
            initializeWorkflow(workflow);
        }
    }, [workflow, initializeWorkflow, isInitialized]);

    // Separate useEffect for cleanup to prevent infinite loops
    useEffect(() => {
        // Clear workflow data when component unmounts
        return () => {
            clearWorkflow();
        };
    }, []); // Empty dependency array ensures this only runs on mount/unmount

    const handleRetry = useCallback(() => {
        refetch();
    }, [refetch]);

    // Loading states
    if (isLoading) {
        return (
            <div className="flex items-center justify-center h-full">
                <Loader2 className="w-5 h-5 animate-spin text-primary" />
                <span className="ml-2">Loading workflow...</span>
            </div>
        );
    }

    // Error state
    if (isError) {
        return (
            <WorkflowError
                message="Unable to fetch workflow data. The server returned an invalid response."
                errorCode="ERR_FETCH_FAILED"
                retry={handleRetry}
                suggestions={[
                    "Check your internet connection",
                    "Verify that the API endpoint is correct",
                    "Contact support if the problem persists",
                ]}
            />
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
                            <span className="ml-2">
                                Initializing workflow editor...
                            </span>
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
