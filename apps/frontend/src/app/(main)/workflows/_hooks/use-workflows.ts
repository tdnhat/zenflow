import { fetchNodeTypes, fetchWorkflows } from "@/api/workflow/workflow-api";
import { useQuery } from "@tanstack/react-query";

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
