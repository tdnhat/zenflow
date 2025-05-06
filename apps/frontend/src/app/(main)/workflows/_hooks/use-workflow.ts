import { createWorkflow, fetchWorkflowById } from "@/api/workflow/workflow-api";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { WorkflowFormValues } from "../_schemas/workflow.schemas";
import toast from "react-hot-toast";
import { workflowKeys } from "./use-workflows";

// Fetch workflow by ID
export const useWorkflow = (workflowId: string) => {
    return useQuery({
        queryKey: workflowKeys.detail(workflowId),
        queryFn: () => fetchWorkflowById(workflowId),
        enabled: !!workflowId,
    });
};

// Create a new workflow
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

// Save workflow
// export const useSaveWorkflow = () => {
//     const queryClient = useQueryClient();
//     return useMutation({
//         mutationFn: (data: WorkflowFormValues)
//     })
// }
