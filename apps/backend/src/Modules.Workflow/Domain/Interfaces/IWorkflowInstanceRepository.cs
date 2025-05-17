using Modules.Workflow.Domain.Core;

namespace Modules.Workflow.Domain.Interfaces.Core
{
    public interface IWorkflowInstanceRepository
    {
        Task<WorkflowExecutionContext> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<WorkflowExecutionContext>> GetByWorkflowIdAsync(Guid workflowId, CancellationToken cancellationToken = default);
        Task SaveAsync(WorkflowExecutionContext context, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}