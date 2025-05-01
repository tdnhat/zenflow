using Microsoft.EntityFrameworkCore;
using Modules.Workflow.Data;
using Modules.Workflow.DDD.Interfaces;

namespace Modules.Workflow.Repositories
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
            await _context.SaveChangesAsync(cancellationToken);
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

        public async Task UpdateAsync(DDD.Entities.WorkflowEdge edge, CancellationToken cancellationToken = default)
        {
            _context.WorkflowEdges.Update(edge);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(DDD.Entities.WorkflowEdge edge, CancellationToken cancellationToken = default)
        {
            _context.WorkflowEdges.Remove(edge);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}