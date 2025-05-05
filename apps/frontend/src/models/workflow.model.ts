/**
 * Represents a workflow node in the system
 */
export interface WorkflowNode {
  id: string;
  nodeType: string;
  nodeKind: string;
  label: string;
  x: number;
  y: number;
  configJson: string;
}

/**
 * Represents a workflow edge connecting two nodes
 */
export interface WorkflowEdge {
  id: string;
  sourceNodeId: string;
  targetNodeId: string;
  edgeType: string;
  label: string;
  conditionJson?: string;
  sourceHandle?: string;
  targetHandle?: string;
}

/**
 * Workflow status values
 */
export enum WorkflowStatus {
  Draft = 'DRAFT',
  Active = 'ACTIVE',
  Archived = 'ARCHIVED',
}

/**
 * Represents a workflow definition
 */
export interface Workflow {
  id: string;
  name: string;
  description: string;
  status: WorkflowStatus;
  nodes: WorkflowNode[];
  edges: WorkflowEdge[];
  createdAt: string;
  updatedAt?: string;
}

/**
 * Workflow execution status values
 */
export enum WorkflowExecutionStatus {
  Pending = 'PENDING',
  Running = 'RUNNING',
  Completed = 'COMPLETED',
  Failed = 'FAILED',
  Cancelled = 'CANCELLED',
}

/**
 * Represents a node execution within a workflow execution
 */
export interface NodeExecution {
  id: string;
  nodeId: string;
  workflowExecutionId: string;
  status: string;
  startedAt: string;
  completedAt?: string;
  error?: string;
  output?: string;
}

/**
 * Represents a workflow execution instance
 */
export interface WorkflowExecution {
  id: string;
  workflowId: string;
  workflowVersion: number;
  status: WorkflowExecutionStatus;
  startedAt: string;
  completedAt?: string;
  errorMessage?: string;
  errorStack?: string;
  errorNodeId?: string;
  outputData?: string;
  nodeExecutions?: NodeExecution[];
} 