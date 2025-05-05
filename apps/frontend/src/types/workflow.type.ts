// Types that mirror backend DTOs
export type WorkflowStatus = "DRAFT" | "ACTIVE" | "ARCHIVED";

export interface WorkflowDto {
    id: string;
    name: string;
    description: string;
    status: WorkflowStatus;
}

export interface WorkflowNodeDto {
    id: string;
    nodeType: string;
    nodeKind: string;
    label: string;
    x: number;
    y: number;
    configJson: string;
}

export interface WorkflowEdgeDto {
    id: string;
    sourceNodeId: string;
    targetNodeId: string;
    label: string;
    edgeType: string;
    conditionJson: string;
    sourceHandle: string;
    targetHandle: string;
}

export interface WorkflowDetailDto {
    id: string;
    name: string;
    description: string;
    status: WorkflowStatus;
    nodes: WorkflowNodeDto[];
    edges: WorkflowEdgeDto[];
}

// Request/Response types for API calls
export interface CreateWorkflowRequest {
    name: string;
    description: string;
}

export interface UpdateWorkflowRequest {
    name: string;
    description: string;
}

export interface AddNodeRequest {
    nodeType: string;
    nodeKind: string;
    x: number;
    y: number;
    label: string;
    configJson: string;
}

export interface UpdateNodeRequest {
    x: number;
    y: number;
    label: string;
    configJson: string;
}

export interface AddEdgeRequest {
    sourceNodeId: string;
    targetNodeId: string;
    label: string;
    edgeType: string;
    conditionJson: string;
    sourceHandle: string;
    targetHandle: string;
}

export interface UpdateEdgeRequest {
    label: string;
    edgeType: string;
    conditionJson: string;
    sourceHandle: string;
    targetHandle: string;
}
