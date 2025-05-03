using Microsoft.EntityFrameworkCore;
using Modules.Workflow.Data;
using Modules.Workflow.DDD.Interfaces;
using Modules.Workflow.Features.WorkflowExecutions.GetWorkflowExecutions;
using ZenFlow.Shared.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
namespace Modules.Workflow.Repositories
{
    public class WorkflowExecutionRepository : IWorkflowExecutionRepository
    {
        private readonly WorkflowDbContext _dbContext;

        public WorkflowExecutionRepository(WorkflowDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<DDD.Entities.WorkflowExecution?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.WorkflowExecutions
                .Include(e => e.NodeExecutions)
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        }

        public async Task<List<DDD.Entities.WorkflowExecution>> GetAllAsync(int skip = 0, int take = 50, CancellationToken cancellationToken = default)
        {
            return await _dbContext.WorkflowExecutions
                .OrderByDescending(e => e.StartedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<DDD.Entities.WorkflowExecution>> GetByWorkflowIdAsync(Guid workflowId, int skip = 0, int take = 50, CancellationToken cancellationToken = default)
        {
            return await _dbContext.WorkflowExecutions
                .Where(e => e.WorkflowId == workflowId)
                .OrderByDescending(e => e.StartedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);
        }

        // New overload implementation
        public async Task<List<DDD.Entities.WorkflowExecution>> GetByWorkflowIdAsync(Guid workflowId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.WorkflowExecutions
                .Where(e => e.WorkflowId == workflowId)
                .OrderByDescending(e => e.StartedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.WorkflowExecutions.CountAsync(cancellationToken);
        }

        public async Task<int> CountByWorkflowIdAsync(Guid workflowId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.WorkflowExecutions
                .Where(e => e.WorkflowId == workflowId)
                .CountAsync(cancellationToken);
        }

        public async Task AddAsync(DDD.Entities.WorkflowExecution execution, CancellationToken cancellationToken = default)
        {
            await _dbContext.WorkflowExecutions.AddAsync(execution, cancellationToken);
        }

        public Task UpdateAsync(DDD.Entities.WorkflowExecution execution, CancellationToken cancellationToken = default)
        {
            _dbContext.WorkflowExecutions.Update(execution);
            return Task.CompletedTask;
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var execution = await GetByIdAsync(id, cancellationToken);
            if (execution != null)
            {
                _dbContext.WorkflowExecutions.Remove(execution);
            }
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<DDD.Entities.WorkflowExecution?> GetByIdWithNodeExecutionsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.WorkflowExecutions
                .Include(e => e.NodeExecutions)
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        }
    }
}