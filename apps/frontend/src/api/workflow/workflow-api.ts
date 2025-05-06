import { WorkflowFormValues, WorkflowSaveValues } from "@/app/(main)/workflows/_schemas/workflow.schemas";
import api from "@/lib/axios";
import { WorkflowDto, WorkflowDetailDto, NodeTypeDto } from "@/types/workflow.type";
import { Edge, Node } from "@xyflow/react";

// Base endpoint for workflows
const WORKFLOWS_ENDPOINT = "/workflows";

// Workflow CRUD operations
export const fetchWorkflows = async (): Promise<WorkflowDto[]> => {
    const response = await api.get(WORKFLOWS_ENDPOINT);
    return response.data;
};

export const fetchWorkflowById = async (
    id: string
): Promise<WorkflowDetailDto> => {
    const response = await api.get(`${WORKFLOWS_ENDPOINT}/${id}`);
    return response.data;
};

export const createWorkflow = async (
    data: WorkflowFormValues
): Promise<WorkflowDto> => {
    const response = await api.post(WORKFLOWS_ENDPOINT, data);
    return response.data;
};

export const fetchNodeTypes = async (): Promise<NodeTypeDto[]> => {
    const response = await api.get("/node-types");
    return response.data;
};

export const saveWorkflow = async (
    id: string,
    nodes: Node[],
    edges: Edge[]
): Promise<WorkflowDetailDto> => {
    // Convert React Flow data to backend DTO format
    const workflowData = mapWorkflowToDto(nodes, edges);
    
    // No longer sending name and description to avoid concurrency issues
    const response = await api.post(`${WORKFLOWS_ENDPOINT}/${id}/save`, workflowData);
    return response.data;
};

// Helper function to convert React Flow nodes and edges to backend DTO format
export const mapWorkflowToDto = (nodes: Node[], edges: Edge[]): WorkflowSaveValues => {
    // Map nodes from React Flow format to backend DTO format
    const mappedNodes = nodes.map(node => {
        // Special handling for manual trigger node
        if (node.type === 'manual-trigger') {
            return {
                id: node.id,
                nodeType: "ManualTriggerActivity",
                nodeKind: "TRIGGER",
                label: String(node.data?.label || "Manual Trigger"),
                x: node.position.x,
                y: node.position.y,
                configJson: JSON.stringify({})
            };
        }

        // Extract node data without internal React Flow properties
        const nodeData = { ...node.data };
        delete nodeData.label;
        delete nodeData.nodeKind;
        delete nodeData.nodeType;
        delete nodeData.id;
        delete nodeData.selected;
        delete nodeData.dragging;
        delete nodeData.targetPosition;
        delete nodeData.sourcePosition;
        delete nodeData.configJson; // Remove any existing configJson to prevent nesting

        // If there's a configJson in the data, parse it and merge with other properties
        let configData: Record<string, unknown> = {};
        if (node.data?.configJson && typeof node.data.configJson === 'string') {
            try {
                const parsed = JSON.parse(node.data.configJson);
                if (typeof parsed === 'object' && parsed !== null) {
                    configData = parsed;
                }
            } catch (e) {
                console.error('Failed to parse configJson:', e);
            }
        }

        // Merge config data with other node data properties
        const finalConfig = {
            ...configData,
            ...nodeData
        };

        return {
            id: node.id,
            nodeType: node.type || "default",
            nodeKind: (node.data?.nodeKind || "ACTION") as string,
            label: String(node.data?.label || node.type || "Unnamed Node"),
            x: node.position.x,
            y: node.position.y,
            configJson: JSON.stringify(finalConfig)
        };
    });

    // Map edges from React Flow format to backend DTO format
    const mappedEdges = edges.map(edge => ({
        id: edge.id,
        sourceNodeId: edge.source,
        targetNodeId: edge.target,
        label: String(edge.label || ""),
        edgeType: edge.type || "default",
        conditionJson: JSON.stringify(edge.data || {}),
        sourceHandle: edge.sourceHandle || "",
        targetHandle: edge.targetHandle || ""
    }));

    return {
        nodes: mappedNodes,
        edges: mappedEdges
    };
};
