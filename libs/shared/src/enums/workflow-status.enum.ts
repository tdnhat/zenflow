/**
 * Represents the current status of a workflow in the ZenFlow system
 */
export enum WorkflowStatus {
  DRAFT = 'DRAFT',           // Workflow is being created/edited
  PUBLISHED = 'ACTIVE',   // Workflow is active and can be executed
  ARCHIVED = 'ARCHIVED',     // Workflow is no longer in use but kept for reference
}