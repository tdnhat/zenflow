import { Workflow } from '../models/workflow.model';
import { WorkflowStatus } from '../enums/workflow-status.enum';

/**
 * Request to create a new workflow
 */
export interface CreateWorkflowRequest {
  name: string;
  description?: string;
  nodes: Array<{
    id: string;
    type: string;
    label: string;
    position: { x: number; y: number };
    data: Record<string, any>;
  }>;
  edges: Array<{
    id: string;
    source: string;
    target: string;
    sourceHandle?: string;
    targetHandle?: string;
    label?: string;
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
    label: string;
    position: { x: number; y: number };
    data: Record<string, any>;
  }>;
  edges?: Array<{
    id: string;
    source: string;
    target: string;
    sourceHandle?: string;
    targetHandle?: string;
    label?: string;
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