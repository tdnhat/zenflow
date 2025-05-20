import { fetchWorkflows, updateWorkflow, fetchWorkflowById } from "@/api/workflow/workflow-api";
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
    updates: () => [...workflowKeys.all, "update"] as const,
    update: (id: string) => [...workflowKeys.updates(), id] as const,
};

// Fetch all workflows
export const useWorkflows = () => {
    return useQuery({
        queryKey: workflowKeys.lists(),
        queryFn: () => fetchWorkflows(),
    });
};

// Fetch node types - commented out since the API function isn't available anymore
// export const useNodeTypes = () => {
//     return useQuery({
//         queryKey: workflowKeys.nodeTypes(),
//         queryFn: () => fetchNodeTypes(),
//     });
// };

// Hook for updating workflow
export const useUpdateWorkflow = (workflowId: string) => {
    const queryClient = useQueryClient();
    const { setSaving, isSaving } = useWorkflowStore();
    
    // Fetch the current workflow to get name and description
    const { data: workflowData } = useQuery({
        queryKey: workflowKeys.detail(workflowId),
        queryFn: () => fetchWorkflowById(workflowId),
        enabled: !!workflowId,
    });
    
    // Use react-query's useMutation for better handling of async operations
    const mutation = useMutation({
        mutationFn: ({ nodes, edges }: { nodes: Node[], edges: Edge[] }) => {
            if (!workflowData) {
                throw new Error("Workflow data not loaded");
            }
            
            return updateWorkflow(
                workflowId, 
                nodes, 
                edges, 
                workflowData.name,
                workflowData.description
            );
        },
        onMutate: () => {
            setSaving(true);
        },
        onSuccess: () => {
            toast.success("Workflow updated successfully!");
            // Invalidate relevant queries to refresh data
            queryClient.invalidateQueries({
                queryKey: workflowKeys.detail(workflowId),
            });
        },
        onError: (error) => {
            console.error("Error updating workflow:", error);
            toast.error("Failed to update workflow. Please try again.");
        },
        onSettled: () => {
            setSaving(false);
        }
    });

    const updateWorkflowData = async (nodes: Node[], edges: Edge[]) => {
        if (!workflowId) {
            toast.error("Workflow ID is missing.");
            return;
        }
        
        if (!workflowData) {
            toast.error("Workflow data not loaded. Please try again.");
            return;
        }
        
        mutation.mutate({ nodes, edges });
    };

    return {
        updateWorkflowData,
        isSaving,
        isError: mutation.isError,
        error: mutation.error,
        isLoading: !workflowData
    };
};

// Keep old hook for backward compatibility, using the new update function
export const useSaveWorkflow = useUpdateWorkflow;
