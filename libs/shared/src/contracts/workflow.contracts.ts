import { Workflow } from '../models/workflow.model';
import { WorkflowStatus } from '../enums/workflow-status.enum';
import { NodeKind } from '../enums/node-kind.enum';

/**
 * Request to create a new workflow
 */
export interface CreateWorkflowRequest {
  name: string;
  description?: string;
  nodes: Array<{
    id: string;
    type: string;
    nodeKind: NodeKind;
    label: string;
    position: { x: number; y: number };
    data: Record<string, any>;
    sourcePosition?: string;
    targetPosition?: string;
  }>;
  edges: Array<{
    id: string;
    source: string;
    target: string;
    sourceHandle?: string;
    targetHandle?: string;
    label?: string;
    type?: string;
    animated?: boolean;
    data?: Record<string, any>;
  }>;
  tags?: string[];
}

/**
 * Response after creating a workflow
 */
export interface CreateWorkflowResponse {
  workflow: Workflow;
}

/**
 * Request to update an existing workflow
 */
export interface UpdateWorkflowRequest {
  id: string;
  name?: string;
  description?: string;
  status?: WorkflowStatus;
  nodes?: Array<{
    id: string;
    type: string;
    nodeKind?: NodeKind;
    label: string;
    position: { x: number; y: number };
    data: Record<string, any>;
    sourcePosition?: string;
    targetPosition?: string;
  }>;
  edges?: Array<{
    id: string;
    source: string;
    target: string;
    sourceHandle?: string;
    targetHandle?: string;
    label?: string;
    type?: string;
    animated?: boolean;
    data?: Record<string, any>;
  }>;
  tags?: string[];
}

/**
 * Response after updating a workflow
 */
export interface UpdateWorkflowResponse {
  workflow: Workflow;
}

/**
 * Request to get workflows with optional filtering
 */
export interface GetWorkflowsRequest {
  status?: WorkflowStatus[];
  tags?: string[];
  createdBy?: string;
  page?: number;
  pageSize?: number;
  sortBy?: string;
  sortDirection?: 'asc' | 'desc';
}

/**
 * Response containing list of workflows
 */
export interface GetWorkflowsResponse {
  workflows: Workflow[];
  totalCount: number;
  page: number;
  pageSize: number;
}

/**
 * Request to create a node in a workflow
 */
export interface CreateNodeRequest {
  nodeType: string;
  nodeKind: NodeKind;
  x: number;
  y: number;
  label: string;
  configJson: string;
  sourcePosition?: string;
  targetPosition?: string;
}

/**
 * Request to create an edge between nodes
 */
export interface CreateEdgeRequest {
  sourceNodeId: string;
  targetNodeId: string;
  label: string;
  edgeType: string;
  conditionJson: string;
  sourceHandle: string;
  targetHandle: string;
  animated?: boolean;
  data?: Record<string, any>;
}

/**
 * Response for a workflow node
 */
export interface WorkflowNodeDto {
  id: string;
  nodeType: string;
  nodeKind: NodeKind;
  label: string;
  x: number;
  y: number;
  configJson: string;
  sourcePosition?: string;
  targetPosition?: string;
}

/**
 * Response for a workflow edge
 */
export interface WorkflowEdgeDto {
  id: string;
  sourceNodeId: string;
  targetNodeId: string;
  label: string;
  edgeType: string;
  conditionJson: string;
  sourceHandle: string;
  targetHandle: string;
  animated?: boolean;
  data?: Record<string, any>;
}

/**
 * Response for a workflow
 */
export interface WorkflowDto {
  id: string;
  name: string;
  description: string;
  status: string;
}

/**
 * Response for a workflow with details
 */
export interface WorkflowDetailDto extends WorkflowDto {
  nodes: WorkflowNodeDto[];
  edges: WorkflowEdgeDto[];
}

/**
 * Browser automation workflow specific configuration
 */
export interface BrowserWorkflowConfigDto {
  searchEngineUrl: string;
  searchTerm: string;
  searchInputSelector: string;
  searchButtonSelector: string;
  searchResultsSelector: string;
  typeDelay: number;
  takeScreenshots: boolean;
}

/**
 * Request to run a browser automation workflow
 */
export interface RunBrowserWorkflowRequest {
  workflowId: string;
  searchTerm?: string;
  takeScreenshots?: boolean;
}

/**
 * Response for a browser automation workflow run
 */
export interface RunBrowserWorkflowResponse {
  executionId: string;
  status: string;
}

/**
 * Request to stop a browser automation workflow
 */
export interface StopBrowserWorkflowRequest {
  executionId: string;
}

/**
 * Response after stopping a browser workflow
 */
export interface StopBrowserWorkflowResponse {
  success: boolean;
  message: string;
}