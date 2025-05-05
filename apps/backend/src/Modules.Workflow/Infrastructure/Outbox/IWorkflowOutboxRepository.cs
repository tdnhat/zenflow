using Modules.Workflow.DDD.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Modules.Workflow.Infrastructure.Outbox
{
    public interface IWorkflowOutboxRepository
    {
        Task AddAsync(WorkflowOutboxMessage message);
        Task StoreAsync(WorkflowOutboxMessage message);
        Task AddInSeparateTransactionAsync(WorkflowOutboxMessage message);
        Task UpdateAsync(WorkflowOutboxMessage message);
        Task<List<WorkflowOutboxMessage>> GetUnprocessedMessagesAsync(int batchSize);
        Task DeleteProcessedMessagesOlderThanAsync(DateTime date);
    }
}