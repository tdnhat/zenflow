import { ExecutionStatus } from '../enums/execution-status.enum';

/**
 * Represents an execution instance of a workflow
 */
export interface WorkflowExecution {
  id: string;
  workflowId: string;
  workflowVersion: number;
  status: ExecutionStatus;
  startedAt: Date;
  completedAt?: Date;
  executedBy: string;
  nodeExecutions: NodeExecution[];
  error?: {
    message: string;
    stack?: string;
    nodeId?: string;
  };
}

/**
 * Represents the execution of a single node in a workflow
 */
export interface NodeExecution {
  nodeId: string;
  status: ExecutionStatus;
  startedAt: Date;
  completedAt?: Date;
  input?: Record<string, any>;
  output?: Record<string, any>;
  error?: string;
  duration?: number; // Duration in milliseconds
}