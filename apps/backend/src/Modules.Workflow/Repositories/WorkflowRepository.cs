using Microsoft.EntityFrameworkCore;
using Modules.Workflow.Data;
using Modules.Workflow.DDD.Interfaces;

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
    }
}
