"use client";

import { Plus } from "lucide-react";
import { Button } from "@/components/ui/button";
import Link from "next/link";
import { WorkflowList } from "./_components/workflow-list";
import { useWorkflows } from "./_hooks/use-workflows";

export default function Workflows() {
    const {
        data: workflows,
        isLoading: isLoadingWorkflows,
        error: errorWorkflows,
    } = useWorkflows();

    if (isLoadingWorkflows) {
        return <div>Loading...</div>;
    }

    if (errorWorkflows) {
        return <div>Error: {errorWorkflows.message}</div>;
    }

    return (
        <div className="space-y-6">
            {/* Header with title and create button */}
            <div className="flex items-center justify-between">
                <h1 className="text-2xl font-bold">Workflows</h1>
                <Link href="/workflows/new">
                    <Button className="flex items-center gap-2">
                        <Plus className="h-4 w-4" />
                        Create Workflow
                    </Button>
                </Link>
            </div>

            {/* Workflows list */}
            <div className="space-y-4">
                {workflows && workflows.length === 0 ? (
                    <div className="bg-card p-12 rounded-lg shadow flex flex-col items-center justify-center gap-4">
                        <p className="text-muted-foreground">
                            No workflows found
                        </p>
                        <Link href="/workflows/new">
                            <Button variant="outline">
                                Create your first workflow
                            </Button>
                        </Link>
                    </div>
                ) : (
                    <WorkflowList workflows={workflows || []} />
                )}
            </div>
        </div>
    );
}
