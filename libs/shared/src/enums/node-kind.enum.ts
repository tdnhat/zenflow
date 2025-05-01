/**
 * Defines the core kinds of nodes that can be used in workflows
 */
export enum NodeKind {
  /**
   * Trigger nodes start a workflow execution (e.g., Webhook, Timer)
   */
  TRIGGER = 'TRIGGER',
  
  /**
   * Action nodes perform a task (e.g., HTTP request, Transform data)
   */
  ACTION = 'ACTION',
  
  /**
   * Condition nodes create branches in workflow execution (e.g., If condition)
   */
  CONDITION = 'CONDITION',
  
  /**
   * Loop nodes iterate over collections or repeat tasks (e.g., ForEach)
   */
  LOOP = 'LOOP',
  
  /**
   * Connector nodes provide integration with external systems (e.g., Database)
   */
  CONNECTOR = 'CONNECTOR'
}