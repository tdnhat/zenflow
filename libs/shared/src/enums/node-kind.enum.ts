/**
 * Represents the different kinds of nodes available in the workflow editor
 */
export enum NodeKind {
  TRIGGER = 'TRIGGER',     // Starting point of a workflow (e.g., webhook, timer, event)
  ACTION = 'ACTION',       // Performs a specific operation (e.g., API call, data transformation)
  CONDITION = 'CONDITION', // Decision point that directs flow based on conditions
  LOOP = 'LOOP',           // Repeats a set of actions based on a collection or condition
  CONNECTOR = 'CONNECTOR'  // Connects to external services (e.g., database, API)
}