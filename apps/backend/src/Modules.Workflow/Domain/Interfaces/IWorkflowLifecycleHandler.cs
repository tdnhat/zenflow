using Modules.Workflow.DDD.Entities;

namespace Modules.Workflow.DDD.Interfaces
{
    public interface IWorkflowLifecycleHandler
    {
        ValueTask WorkflowCompletedAsync(WorkflowCompletedContext context, CancellationToken cancellationToken);
        ValueTask WorkflowFaultedAsync(WorkflowFaultedContext context, CancellationToken cancellationToken);
        ValueTask WorkflowCancelledAsync(WorkflowCancelledContext context, CancellationToken cancellationToken);
    }

    public class WorkflowCompletedContext
    {
        public WorkflowExecution WorkflowExecution { get; set; }
    }

    public class WorkflowFaultedContext
    {
        public WorkflowExecution WorkflowExecution { get; set; }
    }

    public class WorkflowCancelledContext
    {
        public WorkflowExecution WorkflowExecution { get; set; }
    }
}
