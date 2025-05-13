using Microsoft.EntityFrameworkCore;
using Modules.Workflow.DDD.Interfaces;
using Modules.Workflow.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Workflow.Infrastructure.Persistence.Repositories
{
    public class NodeExecutionRepository : INodeExecutionRepository
    {
        private readonly WorkflowDbContext _context;

        public NodeExecutionRepository(WorkflowDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(DDD.Entities.NodeExecution nodeExecution, CancellationToken cancellationToken = default)
        {
            await _context.NodeExecutions.AddAsync(nodeExecution, cancellationToken);
        }

        public async Task<IEnumerable<DDD.Entities.NodeExecution>> GetByWorkflowExecutionIdAsync(Guid workflowExecutionId, CancellationToken cancellationToken = default)
        {
            return await _context.NodeExecutions
                .AsNoTracking()
                .Where(n => n.WorkflowExecutionId == workflowExecutionId)
                .ToListAsync(cancellationToken);
        }

        public async Task<DDD.Entities.NodeExecution?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.NodeExecutions
                .AsNoTracking()
                .FirstOrDefaultAsync(n => n.Id == id, cancellationToken);
        }

        public async Task<DDD.Entities.NodeExecution?> GetByNodeIdAndExecutionIdAsync(Guid nodeId, Guid workflowExecutionId, CancellationToken cancellationToken = default)
        {
            return await _context.NodeExecutions
                .AsNoTracking()
                .FirstOrDefaultAsync(n => n.NodeId == nodeId && n.WorkflowExecutionId == workflowExecutionId, cancellationToken);
        }

        public Task UpdateAsync(DDD.Entities.NodeExecution nodeExecution, CancellationToken cancellationToken = default)
        {
            _context.NodeExecutions.Update(nodeExecution);
            return Task.CompletedTask;
        }

        public async Task AddRangeAsync(IEnumerable<DDD.Entities.NodeExecution> nodeExecutions, CancellationToken cancellationToken = default)
        {
            await _context.NodeExecutions.AddRangeAsync(nodeExecutions, cancellationToken);
        }

        public async Task<List<DDD.Entities.NodeExecution>> GetAllAsync(int skip = 0, int take = 50, CancellationToken cancellationToken = default)
        {
            return await _context.NodeExecutions
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        {
            return await _context.NodeExecutions.CountAsync(cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var nodeExecution = await _context.NodeExecutions.FindAsync(id);
            if (nodeExecution != null)
            {
                _context.NodeExecutions.Remove(nodeExecution);
            }
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}