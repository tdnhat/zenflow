using Modules.Workflow.Domain.Entities;

namespace Modules.Workflow.Domain.Interfaces.Core
{
    public interface IWorkflowRepository
    {
        Task<WorkflowDefinition> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<WorkflowDefinition>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Guid> SaveAsync(WorkflowDefinition workflow, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}