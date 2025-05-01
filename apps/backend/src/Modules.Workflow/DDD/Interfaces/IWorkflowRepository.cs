namespace Modules.Workflow.DDD.Interfaces
{
    public interface IWorkflowRepository
    {
        Task AddAsync(DDD.Entities.Workflow workflow, CancellationToken cancellationToken = default);
    }
}
