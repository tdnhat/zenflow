import { fetchNodeTypes, fetchWorkflows, saveWorkflow } from "@/api/workflow/workflow-api";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Node, Edge } from "@xyflow/react";
import toast from "react-hot-toast";
import { useWorkflowStore } from "@/store/workflow.store";

export const workflowKeys = {
    all: ["workflows"] as const,
    nodeTypes: () => ["node-types"] as const,
    lists: () => [...workflowKeys.all, "list"] as const,
    list: (filters: Record<string, unknown>) =>
        [...workflowKeys.lists(), { filters }] as const,
    details: () => [...workflowKeys.all, "detail"] as const,
    detail: (id: string) => [...workflowKeys.details(), id] as const,
    saves: () => [...workflowKeys.all, "save"] as const,
    save: (id: string) => [...workflowKeys.saves(), id] as const,
};

// Fetch all workflows
export const useWorkflows = () => {
    return useQuery({
        queryKey: workflowKeys.lists(),
        queryFn: () => fetchWorkflows(),
    });
};

// Fetch node types
export const useNodeTypes = () => {
    return useQuery({
        queryKey: workflowKeys.nodeTypes(),
        queryFn: () => fetchNodeTypes(),
    });
};

// Hook for saving workflow
export const useSaveWorkflow = (workflowId: string) => {
    const queryClient = useQueryClient();
    const { setSaving, isSaving } = useWorkflowStore();
    
    // Use react-query's useMutation for better handling of async operations
    const mutation = useMutation({
        mutationFn: ({ nodes, edges }: { nodes: Node[], edges: Edge[] }) => 
            saveWorkflow(workflowId, nodes, edges),
        onMutate: () => {
            setSaving(true);
        },
        onSuccess: () => {
            toast.success("Workflow saved successfully!");
            // Invalidate relevant queries to refresh data
            queryClient.invalidateQueries({
                queryKey: workflowKeys.detail(workflowId),
            });
        },
        onError: (error) => {
            console.error("Error saving workflow:", error);
            toast.error("Failed to save workflow. Please try again.");
        },
        onSettled: () => {
            setSaving(false);
        }
    });

    const saveWorkflowData = async (nodes: Node[], edges: Edge[]) => {
        if (!workflowId) {
            toast.error("Workflow ID is missing.");
            return;
        }
        
        mutation.mutate({ nodes, edges });
    };

    return {
        saveWorkflowData,
        isSaving,
        isError: mutation.isError,
        error: mutation.error
    };
};
