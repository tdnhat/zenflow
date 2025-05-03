using Microsoft.EntityFrameworkCore;
using Modules.Workflow.Data;
using Modules.Workflow.DDD.Interfaces;

namespace Modules.Workflow.Repositories
{
    public class WorkflowNodeRepository : IWorkflowNodeRepository
    {
        private readonly WorkflowDbContext _context;

        public WorkflowNodeRepository(WorkflowDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(DDD.Entities.WorkflowNode node, CancellationToken cancellationToken = default)
        {
            await _context.WorkflowNodes.AddAsync(node, cancellationToken);
        }

        public async Task<DDD.Entities.WorkflowNode?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.WorkflowNodes
                .AsNoTracking()
                .FirstOrDefaultAsync(n => n.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<DDD.Entities.WorkflowNode>> GetByWorkflowIdAsync(Guid workflowId, CancellationToken cancellationToken = default)
        {
            return await _context.WorkflowNodes
                .AsNoTracking()
                .Where(n => n.WorkflowId == workflowId)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<DDD.Entities.WorkflowNode>> GetAllAsync(int skip = 0, int take = 50, CancellationToken cancellationToken = default)
        {
            return await _context.WorkflowNodes
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        {
            return await _context.WorkflowNodes.CountAsync(cancellationToken);
        }

        public Task UpdateAsync(DDD.Entities.WorkflowNode node, CancellationToken cancellationToken = default)
        {
            _context.WorkflowNodes.Update(node);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var node = _context.WorkflowNodes.Find(id);
            if (node != null)
            {
                _context.WorkflowNodes.Remove(node);
            }
            return Task.CompletedTask;
        }

        public Task DeleteAsync(DDD.Entities.WorkflowNode node, CancellationToken cancellationToken = default)
        {
            _context.WorkflowNodes.Remove(node);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}