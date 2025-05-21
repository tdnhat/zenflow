import {
    createWorkflow,
    fetchWorkflowById,
    updateWorkflow,
} from "@/api/workflow/workflow-api";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { WorkflowFormValues } from "../_schemas/workflow.schemas";
import toast from "react-hot-toast";
import { workflowKeys } from "./use-workflows";
import { useWorkflowStore, NodeData } from "@/store/workflow.store";
import { Node, Edge } from "@xyflow/react";
import {
    UpdateWorkflowDefinitionRequest,
    toWorkflowNodes,
    toWorkflowEdges,
    ReactFlowNode,
    ReactFlowEdge,
} from "@/types/workflow.type";

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

export const useUpdateWorkflow = (workflowId: string) => {
    const queryClient = useQueryClient();
    const { setSaving, isSaving } = useWorkflowStore();

    // Fetch the current workflow to get name and description
    const { data: workflowData } = useQuery({
        queryKey: workflowKeys.detail(workflowId),
        queryFn: () => fetchWorkflowById(workflowId),
        enabled: !!workflowId,
    });

    const mutation = useMutation({
        mutationFn: async ({ nodes, edges }: { nodes: Node<NodeData>[]; edges: Edge[] }) => {
            if (!workflowData) {
                throw new Error("Workflow data not loaded. Ensure workflowId is valid and data is fetched.");
            }
            if (!workflowId) {
                throw new Error("Workflow ID is missing");
            }

            const adaptedNodes: ReactFlowNode[] = nodes.map(n => ({
                id: n.id,
                type: n.type || 'customNode',
                position: n.position,
                data: {
                    label: String(n.data.label),
                    nodeType: String(n.data.activityType),
                    activityProperties: n.data.activityProperties as Record<string, unknown>,
                }
            }));

            const workflowNodesDto = toWorkflowNodes(adaptedNodes);
            const workflowEdgesDto = toWorkflowEdges(edges as ReactFlowEdge[]);

            const payload: UpdateWorkflowDefinitionRequest = {
                workflowId: workflowId,
                name: workflowData.name,
                description: workflowData.description,
                nodes: workflowNodesDto,
                edges: workflowEdgesDto,
            };

            return updateWorkflow(payload);
        },
        onMutate: () => {
            setSaving(true);
        },
        onSuccess: () => {
            toast.success("Workflow updated successfully!");
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
        },
    });

    const updateWorkflowData = async (nodes: Node<NodeData>[], edges: Edge[]) => {
        if (!workflowId) {
            toast.error("Workflow ID is missing.");
            return;
        }

        if (!workflowData) {
            toast.error("Workflow data not loaded. Please try again.");
            return;
        }

        const processedNodes: Node<NodeData>[] = nodes.map(node => ({
            ...node,
            position: {
                x: Math.round(node.position.x),
                y: Math.round(node.position.y),
            },
            data: node.data,
        }));

        mutation.mutate({ nodes: processedNodes, edges });
    };

    return {
        updateWorkflowData,
        isSaving,
        isError: mutation.isError,
        error: mutation.error,
        isLoading: !workflowData,
    };
};
