using Modules.Workflow.DDD.Entities;

namespace Modules.Workflow.DDD.Interfaces
{
    public interface IWorkflowOutboxRepository
    {
        Task AddAsync(WorkflowOutboxMessage message);
        Task UpdateAsync(WorkflowOutboxMessage message);
        Task<List<WorkflowOutboxMessage>> GetUnprocessedMessagesAsync(int batchSize);
        Task DeleteProcessedMessagesOlderThanAsync(DateTime date);
    }
}
