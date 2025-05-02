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

/**
 * Request to validate a workflow before execution
 */
export interface ValidateWorkflowRequest {
  workflowId: string;
}

/**
 * Response containing validation results
 */
export interface ValidateWorkflowResponse {
  isValid: boolean;
  validationErrors: Array<{
    nodeId?: string;
    edgeId?: string;
    errorCode: string;
    errorMessage: string;
    severity: 'error' | 'warning';
  }>;
}

/**
 * Browser workflow execution progress update
 */
export interface BrowserExecutionProgress {
  executionId: string;
  status: ExecutionStatus;
  currentNodeId?: string;
  currentNodeName?: string;
  currentActivityName?: string;
  progress?: number; // 0-100 percentage
  screenshot?: string; // Base64 encoded screenshot
  extractedData?: any; // Data extracted from the page
  lastLogMessage?: string;
  error?: string;
}

/**
 * Browser workflow execution result
 */
export interface BrowserExecutionResult {
  executionId: string;
  status: ExecutionStatus;
  startedAt: Date;
  completedAt?: Date;
  duration?: number; // ms
  screenshots: Array<{
    nodeId: string;
    activityName: string;
    timestamp: Date;
    image: string; // Base64 encoded
  }>;
  extractedData: Array<{
    nodeId: string;
    activityName: string;
    timestamp: Date;
    data: any;
  }>;
  error?: {
    message: string;
    stack?: string;
    nodeId?: string;
    screenshot?: string; // Base64 encoded screenshot at error time
  };
}