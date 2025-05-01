using Microsoft.EntityFrameworkCore;
using Modules.Workflow.Data;
using Modules.Workflow.DDD.Interfaces;

namespace Modules.Workflow.Repositories
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
            await _context.SaveChangesAsync(cancellationToken);
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

        public async Task UpdateAsync(DDD.Entities.NodeExecution nodeExecution, CancellationToken cancellationToken = default)
        {
            _context.NodeExecutions.Update(nodeExecution);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task AddRangeAsync(IEnumerable<DDD.Entities.NodeExecution> nodeExecutions, CancellationToken cancellationToken = default)
        {
            await _context.NodeExecutions.AddRangeAsync(nodeExecutions, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}