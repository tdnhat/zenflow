import {
    createWorkflow,
    fetchWorkflows,
    getNodeTypes,
} from "@/api/workflow/workflow-api";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { WorkflowFormValues } from "../_schemas/workflow.schemas";
import { toast } from "react-hot-toast";

export const workflowKeys = {
    all: ["workflows"] as const,
    nodeTypes: () => ["node-types"] as const,
    lists: () => [...workflowKeys.all, "list"] as const,
    list: (filters: Record<string, unknown>) =>
        [...workflowKeys.lists(), { filters }] as const,
    details: () => [...workflowKeys.all, "detail"] as const,
    detail: (id: string) => [...workflowKeys.details(), id] as const,
};

export const useWorkflows = () => {
    return useQuery({
        queryKey: workflowKeys.lists(),
        queryFn: () => fetchWorkflows(),
    });
};

export const useNodeTypes = () => {
    return useQuery({
        queryKey: workflowKeys.nodeTypes(),
        queryFn: () => getNodeTypes(),
    });
};
export const useCreateWorkflow = () => {
    const queryClient = useQueryClient();
    return useMutation({
        mutationFn: (data: WorkflowFormValues) => {
            const promise = createWorkflow(data);

            toast.promise(
                promise,
                {
                    loading: "Creating workflow...",
                    success: "Workflow created successfully!",
                    error: "Failed to create workflow",
                },
                {
                    style: {
                        borderRadius: "10px",
                        background: "var(--background)",
                        color: "var(--foreground)",
                        border: "1px solid var(--border)",
                    },
                    iconTheme: {
                        primary: "var(--primary)",
                        secondary: "var(--primary-foreground)",
                    },
                }
            );

            return promise;
        },
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: workflowKeys.lists() });
        },
    });
};
