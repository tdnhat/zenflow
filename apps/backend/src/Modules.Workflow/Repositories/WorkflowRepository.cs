using Microsoft.EntityFrameworkCore;
using Modules.Workflow.Data;
using Modules.Workflow.DDD.Interfaces;
using Modules.Workflow.DDD.ValueObjects;
using Modules.Workflow.Features.Workflows.GetWorkflows;
using ZenFlow.Shared.Application.Models;

namespace Modules.Workflow.Repositories
{
    public class WorkflowRepository : IWorkflowRepository
    {
        private readonly WorkflowDbContext _context;

        public WorkflowRepository(WorkflowDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(DDD.Entities.Workflow workflow, CancellationToken cancellationToken = default)
        {
            await _context.Workflows.AddAsync(workflow, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<IEnumerable<DDD.Entities.Workflow>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            return await _context.Workflows
                .AsNoTracking()
                .Where(w => w.CreatedBy == userId)
                .ToListAsync(cancellationToken);
        }
        
        public async Task<DDD.Entities.Workflow?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Workflows
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
        }
        
        public async Task UpdateAsync(DDD.Entities.Workflow workflow, CancellationToken cancellationToken = default)
        {
            _context.Workflows.Update(workflow);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<DDD.Entities.Workflow?> GetByIdWithNodesAndEdgesAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Workflows
                .Include(w => w.Nodes)
                .Include(w => w.Edges)
                .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
        }

        public async Task<PaginatedResult<DDD.Entities.Workflow>> GetFilteredAsync(string userId, WorkflowsFilterRequest filter, CancellationToken cancellationToken = default)
        {
            // Start with a query for the user's workflows
            var query = _context.Workflows
                .AsNoTracking()
                .Where(w => w.CreatedBy == userId);

            // Apply filters
            if (!filter.IncludeArchived)
            {
                query = query.Where(w => w.Status != WorkflowStatus.Archived);
            }

            if (!string.IsNullOrWhiteSpace(filter.Status))
            {
                query = query.Where(w => w.Status == filter.Status);
            }

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var searchTerm = filter.SearchTerm.ToLower();
                query = query.Where(w => 
                    w.Name.ToLower().Contains(searchTerm) || 
                    w.Description.ToLower().Contains(searchTerm));
            }

            if (filter.CreatedFrom.HasValue)
            {
                query = query.Where(w => w.CreatedAt >= filter.CreatedFrom.Value);
            }

            if (filter.CreatedTo.HasValue)
            {
                query = query.Where(w => w.CreatedAt <= filter.CreatedTo.Value);
            }

            // Get total count for pagination
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply pagination
            var items = await query
                .OrderByDescending(w => w.CreatedAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync(cancellationToken);

            // Return paginated result
            return new PaginatedResult<DDD.Entities.Workflow>(
                items, 
                totalCount, 
                filter.Page, 
                filter.PageSize);
        }
    }
}
