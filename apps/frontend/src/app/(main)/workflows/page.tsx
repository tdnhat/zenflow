"use client";

import { Plus } from "lucide-react";
import { Button } from "@/components/ui/button";
import { WorkflowList } from "./_components/workflow-list";
import { useWorkflows } from "./_hooks/use-workflows";
import { WorkflowModal } from "./_components/workflow-modal";
import { useWorkflowStore } from "@/store/workflow.store";
import { memo, useCallback } from "react";
import WorkflowListSkeleton from "./_components/workflow-list-skeleton";
import WorkflowError from "./_components/workflow-error";

// Memoize the EmptyState component to prevent unnecessary re-renders
const EmptyState = memo(({ onCreateClick }: { onCreateClick: () => void }) => (
    <div className="bg-card p-12 rounded-lg shadow flex flex-col items-center justify-center gap-4">
        <p className="text-muted-foreground">No workflows found</p>
        <Button variant="outline" onClick={onCreateClick}>
            Create your first workflow
        </Button>
    </div>
));
EmptyState.displayName = "EmptyState";

// Memoize the header component
const PageHeader = memo(({ onCreateClick }: { onCreateClick: () => void }) => (
    <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold">Workflows</h1>
        <Button className="flex items-center gap-2" onClick={onCreateClick}>
            <Plus className="h-4 w-4" />
            Create Workflow
        </Button>
    </div>
));
PageHeader.displayName = "PageHeader";

export default function Workflows() {
    const { openModal } = useWorkflowStore();
    const { data: workflows = [], isLoading, isError, refetch } = useWorkflows();

    const handleCreateClick = () => {
        openModal();
    };

    const handleRetry = useCallback(() => {
        refetch();
    }, [refetch]);

    // Render page content based on loading/error state
    const renderContent = () => {
        if (isLoading) {
            return <WorkflowListSkeleton />;
        }

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

        return workflows.length === 0 ? (
            <EmptyState onCreateClick={handleCreateClick} />
        ) : (
            <WorkflowList workflows={workflows} />
        );
    };

    return (
        <div className="space-y-6 p-8">
            <PageHeader onCreateClick={handleCreateClick} />

            <div className="space-y-4">{renderContent()}</div>

            <WorkflowModal />
        </div>
    );
}
