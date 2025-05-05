"use client";

import { Plus } from "lucide-react";
import { Button } from "@/components/ui/button";
import { WorkflowList } from "./_components/workflow-list";
import { useWorkflows } from "./_hooks/use-workflows";
import { WorkflowModal } from "./_components/workflow-modal";
import { useWorkflowStore } from "@/store/workflow.store";
import { memo } from "react";
import WorkflowListSkeleton from "./_components/workflow-list-skeleton";

// Memoize the EmptyState component to prevent unnecessary re-renders
const EmptyState = memo(({ onCreateClick }: { onCreateClick: () => void }) => (
    <div className="bg-card p-12 rounded-lg shadow flex flex-col items-center justify-center gap-4">
        <p className="text-muted-foreground">No workflows found</p>
        <Button variant="outline" onClick={onCreateClick}>
            Create your first workflow
        </Button>
    </div>
));
EmptyState.displayName = 'EmptyState';

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
PageHeader.displayName = 'PageHeader';

export default function Workflows() {
    const { openModal } = useWorkflowStore();
    const {
        data: workflows = [],
        isLoading,
        error,
    } = useWorkflows();

    const handleCreateClick = () => {
        openModal();
    };

    // Render page content based on loading/error state
    const renderContent = () => {
        if (isLoading) {
            return <WorkflowListSkeleton />;
        }

        if (error) {
            return (
                <div className="p-6 rounded-lg border border-destructive bg-destructive/10 text-destructive">
                    <h2 className="text-lg font-semibold mb-2">Error</h2>
                    <p>{error.message || 'Failed to load workflows'}</p>
                </div>
            );
        }

        return workflows.length === 0 ? (
            <EmptyState onCreateClick={handleCreateClick} />
        ) : (
            <WorkflowList workflows={workflows} />
        );
    };

    return (
        <div className="space-y-6">
            <PageHeader onCreateClick={handleCreateClick} />
            
            <div className="space-y-4">
                {renderContent()}
            </div>

            <WorkflowModal />
        </div>
    );
}
