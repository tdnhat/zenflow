import { WorkflowFormValues } from "@/app/(main)/workflows/_schemas/workflow.schemas";
import api from "@/lib/axios";
import { 
    ConditionDto, 
    InputMappingDto, 
    OutputMappingDto, 
    WorkflowDefinitionDto, 
    WorkflowEdgeDto, 
    WorkflowNodeDto,
    UpdateWorkflowDefinitionRequest
} from "@/types/workflow.type";
import { Edge, Node } from "@xyflow/react";

// Base endpoint for workflows
const WORKFLOWS_ENDPOINT = "/workflows";

// Workflow CRUD operations
export const fetchWorkflows = async (): Promise<WorkflowDefinitionDto[]> => {
    const response = await api.get(WORKFLOWS_ENDPOINT);
    return response.data;
};

export const fetchWorkflowById = async (
    id: string
): Promise<WorkflowDefinitionDto> => {
    const response = await api.get(`${WORKFLOWS_ENDPOINT}/${id}`);
    return response.data;
};

export const createWorkflow = async (
    data: WorkflowFormValues
): Promise<string> => {
    // Include empty nodes and edges arrays when creating a new workflow
    const workflowData = {
        ...data,
        nodes: [],
        edges: []
    };
    
    const response = await api.post(WORKFLOWS_ENDPOINT, workflowData);
    return response.data.id;
};

// Define interface for mapping data structure
interface MappingData {
    sourceNodeId?: string;
    sourceProperty?: string;
    targetProperty?: string;
    targetNodeId?: string;
}

// Convert React Flow nodes to the format expected by backend
export const convertNodesToBackendFormat = (nodes: Node[]): WorkflowNodeDto[] => {
    return nodes.map(node => {
        // Extract data from the node
        const nodeData = node.data || {};
        
        // Create a base node with required properties
        const baseNode: WorkflowNodeDto = {
            id: node.id,
            name: String(nodeData.label || node.type || "Unnamed Node"),
            activityType: String(node.type || ""),
            // Convert node properties to the format backend expects
            activityProperties: nodeData.activityProperties || Object.create(null),
            position: {
                x: node.position.x,
                y: node.position.y
            }
        };
        
        // Add optional input mappings if they exist and are correctly formatted
        if (nodeData.inputMappings && Array.isArray(nodeData.inputMappings)) {
            baseNode.inputMappings = nodeData.inputMappings.map((mapping: MappingData): InputMappingDto => ({
                sourceNodeId: mapping.sourceNodeId || "",
                sourceProperty: mapping.sourceProperty || "",
                targetProperty: mapping.targetProperty || ""
            }));
        }
        
        // Add optional output mappings if they exist and are correctly formatted
        if (nodeData.outputMappings && Array.isArray(nodeData.outputMappings)) {
            baseNode.outputMappings = nodeData.outputMappings.map((mapping: MappingData): OutputMappingDto => ({
                sourceProperty: mapping.sourceProperty || "",
                targetProperty: mapping.targetProperty || "",
                ...(mapping.targetNodeId ? { targetNodeId: mapping.targetNodeId } : {})
            }));
        }
        
        return baseNode;
    });
};

// Convert React Flow edges to the format expected by backend
export const convertEdgesToBackendFormat = (edges: Edge[]): WorkflowEdgeDto[] => {
    return edges.map(edge => {
        // Basic properties for all edges
        const result: WorkflowEdgeDto = {
            id: edge.id,
            source: edge.source,
            target: edge.target
        };

        // Add condition if it exists and has the expected properties
        if (edge.data?.condition && 
            typeof edge.data.condition === 'object' && 
            edge.data.condition !== null) {
            
            // Get condition data safely
            const conditionData = edge.data.condition as Record<string, unknown>;
            
            // Create a valid condition - only set expression, type is optional
            const condition: ConditionDto = {
                expression: typeof conditionData.expression === 'string' ? conditionData.expression : "true"
            };
            
            // Add type if it exists
            if (typeof conditionData.type === 'string') {
                condition.type = conditionData.type;
            }
            
            result.condition = condition;
        }

        return result;
    });
};

export const updateWorkflow = async (
    payload: UpdateWorkflowDefinitionRequest
): Promise<string> => {
    // The payload already contains nodes and edges in the correct DTO format,
    // and includes the name, and description, and now workflowId.
    
    // The backend endpoint expects the workflow ID in the route AND in the body.
    // The payload includes `workflowId` which is the workflowId.
    const response = await api.put(`${WORKFLOWS_ENDPOINT}/${payload.workflowId}`, payload);
    return response.data.id; // Assuming the backend returns { id: string } upon successful update
};
