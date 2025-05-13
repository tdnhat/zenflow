using Microsoft.EntityFrameworkCore;
using Modules.Workflow.DDD.Interfaces;
using Modules.Workflow.Infrastructure.Persistence;

namespace Modules.Workflow.Infrastructure.Persistence.Repositories
{
    public class WorkflowEdgeRepository : IWorkflowEdgeRepository
    {
        private readonly WorkflowDbContext _context;

        public WorkflowEdgeRepository(WorkflowDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(DDD.Entities.WorkflowEdge edge, CancellationToken cancellationToken = default)
        {
            await _context.WorkflowEdges.AddAsync(edge, cancellationToken);
        }

        public async Task<DDD.Entities.WorkflowEdge?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.WorkflowEdges
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<DDD.Entities.WorkflowEdge>> GetByWorkflowIdAsync(Guid workflowId, CancellationToken cancellationToken = default)
        {
            return await _context.WorkflowEdges
                .AsNoTracking()
                .Where(e => e.WorkflowId == workflowId)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<DDD.Entities.WorkflowEdge>> GetAllAsync(int skip = 0, int take = 50, CancellationToken cancellationToken = default)
        {
            return await _context.WorkflowEdges
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        {
            return await _context.WorkflowEdges.CountAsync(cancellationToken);
        }

        public Task UpdateAsync(DDD.Entities.WorkflowEdge edge, CancellationToken cancellationToken = default)
        {
            _context.WorkflowEdges.Update(edge);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var edge = _context.WorkflowEdges.Find(id);
            if (edge != null)
            {
                _context.WorkflowEdges.Remove(edge);
            }
            return Task.CompletedTask;
        }

        public Task DeleteAsync(DDD.Entities.WorkflowEdge edge, CancellationToken cancellationToken = default)
        {
            _context.WorkflowEdges.Remove(edge);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}