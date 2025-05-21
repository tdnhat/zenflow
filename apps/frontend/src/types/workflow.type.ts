// Updated Types that mirror refactored backend DTOs
export type WorkflowStatus =
    | "NotStarted"
    | "Running"
    | "Completed"
    | "Failed"
    | "Cancelled"
    | "Suspended";
export type NodeStatus =
    | "NotStarted"
    | "Pending"
    | "Running"
    | "Completed"
    | "Failed"
    | "Skipped"
    | "Cancelled";

// Core workflow definition types
export interface WorkflowDefinitionDto {
    id: string;
    name: string;
    description: string;
    version: number;
    createdAt: string;
    updatedAt: string;
    nodes: WorkflowNodeDto[];
    edges: WorkflowEdgeDto[];
}

export interface WorkflowNodeDto {
    id: string;
    name: string;
    activityType: string;
    activityProperties: Record<string, unknown>;
    position: {
        x: number;
        y: number;
    };
    inputMappings?: InputMappingDto[];
    outputMappings?: OutputMappingDto[];
}

export interface WorkflowEdgeDto {
    id: string;
    source: string;
    target: string;
    condition?: ConditionDto;
}

export interface InputMappingDto {
    sourceNodeId: string;
    sourceProperty: string;
    targetProperty: string;
}

export interface OutputMappingDto {
    targetNodeId?: string;
    sourceProperty: string;
    targetProperty: string;
}

export interface ConditionDto {
    type?: string;
    expression: string;
}

export interface PositionDto {
    x: number;
    y: number;
}

// Activity properties for different node types
export interface HttpRequestActivityProperties {
    url: string;
    method: string;
    headers?: Record<string, string>;
    body?: string;
    timeout?: number;
}

export interface SendEmailActivityProperties {
    to: string[];
    cc?: string[];
    bcc?: string[];
    from?: string;
    subject: string;
    body: string;
    isHtml: boolean;
}

export interface ArticleSummarizationActivityProperties {
    maxLength?: number;
}

export interface DelayActivityProperties {
    delaySeconds: number;
}

export interface PlaywrightActivityProperties {
    url?: string;
    actions: PlaywrightActionDto[];
}

export interface PlaywrightActionDto {
    type: "navigate" | "click" | "type" | "select" | "screenshot" | "extract";
    selector?: string;
    value?: string;
    options?: Record<string, unknown>;
}

// Workflow execution types
export interface WorkflowRunStatusDto {
    workflowRunId: string;
    workflowId: string;
    workflowName: string;
    status: WorkflowStatus;
    startedAt?: string;
    completedAt?: string;
    nodes: NodeStatusDto[];
}

export interface NodeStatusDto {
    nodeId: string;
    name: string;
    activityType: string;
    status: NodeStatus;
    startedAt?: string;
    completedAt?: string;
    durationMs?: number;
    error?: string;
    logs?: string[];
}

// Request/Response types for API calls
export interface CreateWorkflowDefinitionRequest {
    name: string;
    description: string;
    nodes: WorkflowNodeDto[];
    edges: WorkflowEdgeDto[];
}

export interface UpdateWorkflowDefinitionRequest {
    workflowId: string;
    name: string;
    description: string;
    nodes: WorkflowNodeDto[];
    edges: WorkflowEdgeDto[];
}

export interface RunWorkflowRequest {
    workflowId: string;
    variables?: Record<string, unknown>;
}

export interface RunWorkflowResponse {
    workflowRunId: string;
}

export interface PagedResult<T> {
    page: number;
    pageSize: number;
    totalPages: number;
    totalCount: number;
    items: T[];
}

// Utility type for React Flow integration
export interface ReactFlowNode {
    id: string;
    type: string;
    position: {
        x: number;
        y: number;
    };
    data: {
        label: string;
        nodeType: string;
        activityProperties: Record<string, unknown>;
    };
}

export interface ReactFlowEdge {
    id: string;
    source: string;
    target: string;
    label?: string;
    type?: string;
    data?: {
        condition?: ConditionDto;
    };
}

// Conversion functions between backend DTOs and React Flow elements
export function toReactFlowNodes(nodes: WorkflowNodeDto[]): ReactFlowNode[] {
    return nodes.map((node) => ({
        id: node.id,
        type: "customNode",
        position: node.position,
        data: {
            label: node.name,
            nodeType: node.activityType,
            activityProperties: node.activityProperties,
        },
    }));
}

export function toReactFlowEdges(edges: WorkflowEdgeDto[]): ReactFlowEdge[] {
    return edges.map((edge) => ({
        id: edge.id,
        source: edge.source,
        target: edge.target,
        type: "customEdge",
        data: {
            condition: edge.condition,
        },
    }));
}

export function toWorkflowNodes(nodes: ReactFlowNode[]): WorkflowNodeDto[] {
    return nodes.map((node) => ({
        id: node.id,
        name: node.data.label,
        activityType: node.data.nodeType,
        activityProperties: node.data.activityProperties,
        position: node.position,
    }));
}

export function toWorkflowEdges(edges: ReactFlowEdge[]): WorkflowEdgeDto[] {
    return edges.map((edge) => ({
        id: edge.id,
        source: edge.source,
        target: edge.target,
        condition: edge.data?.condition,
    }));
}
