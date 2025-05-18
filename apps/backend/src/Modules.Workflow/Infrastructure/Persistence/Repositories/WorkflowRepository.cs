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
            }

            // Check if this is an update operation
            var existingWorkflow = await _dbContext.WorkflowDefinitions
                .Include(w => w.Nodes)
                .Include(w => w.Edges)
                .FirstOrDefaultAsync(w => w.Id == workflow.Id, cancellationToken);

            if (existingWorkflow != null)
            {
                // Update operation - first remove existing nodes and edges
                _dbContext.WorkflowNodes.RemoveRange(existingWorkflow.Nodes);
                _dbContext.WorkflowEdges.RemoveRange(existingWorkflow.Edges);
                
                // Update basic properties
                existingWorkflow.Name = workflow.Name;
                existingWorkflow.Description = workflow.Description;
                existingWorkflow.Version = workflow.Version;
                existingWorkflow.UpdatedAt = DateTime.UtcNow;
                
                // Add the new nodes and edges
                foreach (var node in workflow.Nodes)
                {
                    node.WorkflowId = workflow.Id;
                    _dbContext.WorkflowNodes.Add(node);
                }

                foreach (var edge in workflow.Edges)
                {
                    edge.WorkflowId = workflow.Id;
                    _dbContext.WorkflowEdges.Add(edge);
                }
                
                // Save all changes in a single transaction
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            else
            {
                // Create operation - add everything in one go
                workflow.CreatedAt = DateTime.UtcNow;
                
                // Ensure all nodes and edges have the correct workflow ID
                foreach (var node in workflow.Nodes)
                {
                    node.WorkflowId = workflow.Id;
                }

                foreach (var edge in workflow.Edges)
                {
                    edge.WorkflowId = workflow.Id;
                }
                
                // Add the workflow and all its related entities
                _dbContext.WorkflowDefinitions.Add(workflow);
                
                // Save everything in a single transaction
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

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