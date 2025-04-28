/**
 * Represents the status of a workflow execution
 */
export enum ExecutionStatus {
  PENDING = 'PENDING',     // Execution is pending to start
  RUNNING = 'RUNNING',     // Execution is currently in progress
  COMPLETED = 'COMPLETED', // Execution completed successfully
  FAILED = 'FAILED',       // Execution failed with an error
  CANCELLED = 'CANCELLED'  // Execution was manually cancelled
}