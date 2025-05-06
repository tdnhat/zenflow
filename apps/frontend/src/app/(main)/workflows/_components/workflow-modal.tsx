"use client";
import { useWorkflowStore } from "@/store/workflow.store";
import {
    Dialog,
    DialogContent,
    DialogDescription,
    DialogFooter,
    DialogHeader,
    DialogTitle,
} from "@/components/ui/dialog";
import { WorkflowCreateForm, useCreateWorkflow } from "./workflow-create-form";
import { Button } from "@/components/ui/button";
import { Loader2 } from "lucide-react";
import { useEffect } from "react";

export function WorkflowModal() {
    const { isOpen, closeModal, error, clearError } = useWorkflowStore();
    const { isPending } = useCreateWorkflow();
    
    // Clear any errors when modal opens or closes
    useEffect(() => {
        clearError();
    }, [isOpen, clearError]);

    return (
        <Dialog open={isOpen} onOpenChange={closeModal}>
            <DialogContent className="sm:max-w-[425px]">
                <DialogHeader>
                    <DialogTitle>Create New Workflow</DialogTitle>
                    <DialogDescription>
                        Create a new automation workflow to connect your apps
                        and services.
                    </DialogDescription>
                </DialogHeader>

                <WorkflowCreateForm />

                {error && (
                    <div className="text-sm text-destructive mt-2">
                        {error}
                    </div>
                )}

                <DialogFooter className="pt-4">
                    <Button
                        variant="outline"
                        type="button"
                        onClick={closeModal}
                        disabled={isPending}
                    >
                        Cancel
                    </Button>
                    <Button 
                        type="submit" 
                        disabled={isPending}
                        form="workflow-create-form"
                    >
                        {isPending ? (
                            <>
                                <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                                Creating...
                            </>
                        ) : (
                            "Create Workflow"
                        )}
                    </Button>
                </DialogFooter>
            </DialogContent>
        </Dialog>
    );
}
