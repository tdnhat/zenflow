namespace Modules.Workflow.Enums
{
    /// <summary>
    /// Defines the core kinds of nodes that can be used in workflows
    /// </summary>
    public static class NodeKind
    {
        /// <summary>
        /// Trigger nodes start a workflow execution (e.g., Webhook, Timer)
        /// </summary>
        public const string TRIGGER = "TRIGGER";
        
        /// <summary>
        /// Action nodes perform a task (e.g., HTTP request, Transform data)
        /// </summary>
        public const string ACTION = "ACTION";
        
        /// <summary>
        /// Condition nodes create branches in workflow execution (e.g., If condition)
        /// </summary>
        public const string CONDITION = "CONDITION";
        
        /// <summary>
        /// Loop nodes iterate over collections or repeat tasks (e.g., ForEach)
        /// </summary>
        public const string LOOP = "LOOP";
        
        /// <summary>
        /// Connector nodes provide integration with external systems (e.g., Database)
        /// </summary>
        public const string CONNECTOR = "CONNECTOR";
    }
}