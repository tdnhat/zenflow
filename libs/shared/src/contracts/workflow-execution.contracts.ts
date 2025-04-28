import { ExecutionStatus } from '../enums/execution-status.enum';
import { WorkflowExecution } from '../models/workflow-execution.model';

/**
 * Request to execute a workflow
 */
export interface ExecuteWorkflowRequest {
  workflowId: string;
  input?: Record<string, any>;
}

/**
 * Response after initiating a workflow execution
 */
export interface ExecuteWorkflowResponse {
  executionId: string;
  status: ExecutionStatus;
  startedAt: Date;
}

/**
 * Request to get execution details
 */
export interface GetWorkflowExecutionRequest {
  executionId: string;
}

/**
 * Response with execution details
 */
export interface GetWorkflowExecutionResponse {
  execution: WorkflowExecution;
}

/**
 * Request to get execution history for a workflow
 */
export interface GetWorkflowExecutionsRequest {
  workflowId: string;
  status?: ExecutionStatus[];
  from?: Date;
  to?: Date;
  page?: number;
  pageSize?: number;
}

/**
 * Response with execution history
 */
export interface GetWorkflowExecutionsResponse {
  executions: WorkflowExecution[];
  totalCount: number;
  page: number;
  pageSize: number;
}

/**
 * Request to cancel a running workflow execution
 */
export interface CancelWorkflowExecutionRequest {
  executionId: string;
  reason?: string;
}

/**
 * Response after canceling a workflow execution
 */
export interface CancelWorkflowExecutionResponse {
  execution: WorkflowExecution;
  cancelled: boolean;
}