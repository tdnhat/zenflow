import apiClient from "./apiClient";
import {
    WorkflowDetailDto,
    WorkflowNodeDto,
    WorkflowEdgeDto,
    CreateNodeRequest,
    CreateEdgeRequest,
} from "@shared/contracts/workflow.contracts";

/**
 * API client for workflow-related operations
 */
export const workflowApi = {
    /**
     * Get a workflow by ID including its nodes and edges
     */
    getWorkflowById: async (workflowId: string): Promise<WorkflowDetailDto> => {
        return apiClient.get(`/api/workflows/${workflowId}`);
    },

    /**
     * Get all nodes for a workflow
     */
    getWorkflowNodes: async (
        workflowId: string
    ): Promise<WorkflowNodeDto[]> => {
        return apiClient.get(`/api/workflows/${workflowId}/nodes`);
    },

    /**
     * Get all edges for a workflow
     */
    getWorkflowEdges: async (
        workflowId: string
    ): Promise<WorkflowEdgeDto[]> => {
        return apiClient.get(`/api/workflows/${workflowId}/edges`);
    },

    /**
     * Create a new node in a workflow
     */
    createNode: async (
        workflowId: string,
        node: CreateNodeRequest
    ): Promise<WorkflowNodeDto> => {
        return apiClient.post(`/api/workflows/${workflowId}/nodes`, node);
    },

    /**
     * Update an existing node
     */
    updateNode: async (
        workflowId: string,
        nodeId: string,
        node: Partial<CreateNodeRequest>
    ): Promise<WorkflowNodeDto> => {
        return apiClient.put(
            `/api/workflows/${workflowId}/nodes/${nodeId}`,
            node
        );
    },

    /**
     * Delete a node
     */
    deleteNode: async (workflowId: string, nodeId: string): Promise<void> => {
        return apiClient.delete(`/api/workflows/${workflowId}/nodes/${nodeId}`);
    },

    /**
     * Create a new edge between nodes
     */
    createEdge: async (
        workflowId: string,
        edge: CreateEdgeRequest
    ): Promise<WorkflowEdgeDto> => {
        return apiClient.post(`/api/workflows/${workflowId}/edges`, edge);
    },

    /**
     * Delete an edge
     */
    deleteEdge: async (workflowId: string, edgeId: string): Promise<void> => {
        return apiClient.delete(`/api/workflows/${workflowId}/edges/${edgeId}`);
    },

    /**
     * Get all available node types
     */
    getNodeTypes: async () => {
        return apiClient.get("/api/node-types");
    },

    /**
     * Get node types by category
     */
    getNodeTypesByCategory: async (category: string) => {
        return apiClient.get(`/api/node-types/categories/${category}`);
    },

    /**
     * Validate a workflow
     */
    validateWorkflow: async (
        workflowId: string
    ): Promise<ValidateWorkflowResponse> => {
        return apiClient.post(`/api/workflows/${workflowId}/validate`, {
            workflowId,
        });
    },
};
