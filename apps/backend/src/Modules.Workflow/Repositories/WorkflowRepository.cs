using Microsoft.EntityFrameworkCore;
using Modules.Workflow.Data;
using Modules.Workflow.DDD.ValueObjects;
using Modules.Workflow.Features.Workflows.GetWorkflows;
using ZenFlow.Shared.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Modules.Workflow.DDD.Interfaces;

namespace Modules.Workflow.Repositories
{
    public class WorkflowRepository : IWorkflowRepository
    {
        private readonly WorkflowDbContext _dbContext;

        public WorkflowRepository(WorkflowDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<DDD.Entities.Workflow?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Workflows.FindAsync(id, cancellationToken);
        }

        public async Task<DDD.Entities.Workflow?> GetByIdWithNodesAndEdgesAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Workflows
                .Include(w => w.Nodes)
                .Include(w => w.Edges)
                .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
        }

        public async Task<List<DDD.Entities.Workflow>> GetAllAsync(int skip = 0, int take = 50, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Workflows
                .OrderByDescending(w => w.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.Workflows.CountAsync(cancellationToken);
        }

        public async Task AddAsync(DDD.Entities.Workflow workflow, CancellationToken cancellationToken = default)
        {
            await _dbContext.Workflows.AddAsync(workflow, cancellationToken);
        }

        public Task UpdateAsync(DDD.Entities.Workflow workflow, CancellationToken cancellationToken = default)
        {
            _dbContext.Workflows.Update(workflow);
            return Task.CompletedTask;
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var workflow = await GetByIdAsync(id, cancellationToken);
            if (workflow != null)
            {
                _dbContext.Workflows.Remove(workflow);
            }
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<IEnumerable<DDD.Entities.Workflow>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Workflows
                .AsNoTracking()
                .Where(w => w.CreatedBy == userId)
                .ToListAsync(cancellationToken);
        }
    }
}
