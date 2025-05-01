using Microsoft.EntityFrameworkCore;
using Modules.Workflow.Data;
using Modules.Workflow.DDD.Interfaces;
using Modules.Workflow.Features.WorkflowExecutions.GetWorkflowExecutions;
using ZenFlow.Shared.Application.Models;

namespace Modules.Workflow.Repositories
{
    public class WorkflowExecutionRepository : IWorkflowExecutionRepository
    {
        private readonly WorkflowDbContext _context;

        public WorkflowExecutionRepository(WorkflowDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(DDD.Entities.WorkflowExecution workflowExecution, CancellationToken cancellationToken = default)
        {
            await _context.WorkflowExecutions.AddAsync(workflowExecution, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<IEnumerable<DDD.Entities.WorkflowExecution>> GetByWorkflowIdAsync(Guid workflowId, CancellationToken cancellationToken = default)
        {
            return await _context.WorkflowExecutions
                .AsNoTracking()
                .Where(e => e.WorkflowId == workflowId)
                .OrderByDescending(e => e.StartedAt)
                .ToListAsync(cancellationToken);
        }
        
        public async Task<DDD.Entities.WorkflowExecution?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.WorkflowExecutions
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        }
        
        public async Task UpdateAsync(DDD.Entities.WorkflowExecution workflowExecution, CancellationToken cancellationToken = default)
        {
            _context.WorkflowExecutions.Update(workflowExecution);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<DDD.Entities.WorkflowExecution?> GetByIdWithNodeExecutionsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.WorkflowExecutions
                .Include(e => e.NodeExecutions)
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<DDD.Entities.WorkflowExecution>> GetFilteredAsync(WorkflowExecutionsFilterRequest filter, CancellationToken cancellationToken = default)
        {
            // Start with a base query
            var query = _context.WorkflowExecutions
                .AsNoTracking();

            // Apply basic filters
            if (filter.WorkflowId.HasValue)
            {
                query = query.Where(e => e.WorkflowId == filter.WorkflowId.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.Status))
            {
                query = query.Where(e => e.Status == filter.Status);
            }

            // Return the filtered results
            return await query
                .OrderByDescending(e => e.StartedAt)
                .ToListAsync(cancellationToken);
        }
    }
}