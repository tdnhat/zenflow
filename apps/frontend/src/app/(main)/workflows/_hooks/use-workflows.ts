import { fetchWorkflows } from "@/api/workflow/workflow-api";
import { useQuery } from "@tanstack/react-query";

export const workflowKeys = {
    all: ["workflows"] as const,
    lists: () => [...workflowKeys.all, "list"] as const,
    list: (filters: any) => [...workflowKeys.lists(), { filters }] as const,
    details: () => [...workflowKeys.all, "detail"] as const,
    detail: (id: string) => [...workflowKeys.details(), id] as const,
};

export const useWorkflows = () => {
    return useQuery({
        queryKey: workflowKeys.lists(),
        queryFn: () => fetchWorkflows(),
    });
};
