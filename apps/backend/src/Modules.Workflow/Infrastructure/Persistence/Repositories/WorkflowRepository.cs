using Microsoft.EntityFrameworkCore;
using Modules.Workflow.Domain.Interfaces.Core;
using Modules.Workflow.Domain.Entities;

namespace Modules.Workflow.Infrastructure.Persistence.Repositories
{
    public class WorkflowRepository : IWorkflowRepository
    {
        private readonly WorkflowDbContext _dbContext;

        public WorkflowRepository(WorkflowDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<WorkflowDefinition> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.WorkflowDefinitions
                .Include(w => w.Nodes)
                .Include(w => w.Edges)
                .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);

            return entity;
        }

        public async Task<IEnumerable<WorkflowDefinition>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.WorkflowDefinitions
                .Include(w => w.Nodes)
                .Include(w => w.Edges)
                .ToListAsync(cancellationToken);
        }

        public async Task<Guid> SaveAsync(WorkflowDefinition workflow, CancellationToken cancellationToken = default)
        {
            if (workflow.Id == Guid.Empty)
            {
                workflow.Id = Guid.NewGuid();
                await _dbContext.WorkflowDefinitions.AddAsync(workflow, cancellationToken);
            }
            else
            {
                // Handle updates - first remove existing nodes and edges
                var existingWorkflow = await _dbContext.WorkflowDefinitions
                    .Include(w => w.Nodes)
                    .Include(w => w.Edges)
                    .FirstOrDefaultAsync(w => w.Id == workflow.Id, cancellationToken);

                if (existingWorkflow != null)
                {
                    _dbContext.WorkflowNodes.RemoveRange(existingWorkflow.Nodes);
                    _dbContext.WorkflowEdges.RemoveRange(existingWorkflow.Edges);

                    // Update properties
                    _dbContext.Entry(existingWorkflow).CurrentValues.SetValues(workflow);

                    // Add new nodes and edges
                    foreach (var node in workflow.Nodes)
                    {
                        node.WorkflowId = workflow.Id;
                        await _dbContext.WorkflowNodes.AddAsync(node, cancellationToken);
                    }

                    foreach (var edge in workflow.Edges)
                    {
                        edge.WorkflowId = workflow.Id;
                        await _dbContext.WorkflowEdges.AddAsync(edge, cancellationToken);
                    }
                }
                else
                {
                    _dbContext.WorkflowDefinitions.Update(workflow);
                }
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            return workflow.Id;
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var workflow = await _dbContext.WorkflowDefinitions
                .Include(w => w.Nodes)
                .Include(w => w.Edges)
                .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);

            if (workflow != null)
            {
                _dbContext.WorkflowNodes.RemoveRange(workflow.Nodes);
                _dbContext.WorkflowEdges.RemoveRange(workflow.Edges);
                _dbContext.WorkflowDefinitions.Remove(workflow);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }
    }
}