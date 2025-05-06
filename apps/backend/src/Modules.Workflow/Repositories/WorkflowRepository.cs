using MassTransit;
using Microsoft.EntityFrameworkCore;
using Modules.Workflow.Data;
using Modules.Workflow.DDD.Interfaces;
using Modules.Workflow.Infrastructure.Events;
using ZenFlow.Shared.Domain;
using Modules.Workflow.Dtos;
using Modules.Workflow.DDD.Entities;
using Modules.Workflow.DDD.ValueObjects;
using System.Text.Json;

namespace Modules.Workflow.Repositories
{
    public class WorkflowRepository : IWorkflowRepository
    {
        private readonly WorkflowDbContext _dbContext;
        private readonly IWorkflowDomainEventService _domainEventService;

        public WorkflowRepository(WorkflowDbContext dbContext, IWorkflowDomainEventService domainEventService)
        {
            _dbContext = dbContext;
            _domainEventService = domainEventService;
        }

        public async Task<DDD.Entities.Workflow?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Workflows.FindAsync(new object[] { id }, cancellationToken);
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

        public async Task UpdateAsync(DDD.Entities.Workflow workflow, CancellationToken cancellationToken = default)
        {
            try
            {
                _dbContext.Workflows.Update(workflow);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new Exception("The workflow has been modified by another user. Please refresh and try again.", ex);
            }
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
            // Collect domain events before saving
            var domainEvents = CollectDomainEvents();

            // Process domain events - this will add them to the DbContext but not save yet
            if (domainEvents.Any())
            {
                foreach (var domainEvent in domainEvents)
                {
                    await _domainEventService.PublishAsync(domainEvent);
                }
            }

            // Now save everything in a single transaction
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<IEnumerable<DDD.Entities.Workflow>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Workflows
                .AsNoTracking()
                .Where(w => w.CreatedBy == userId)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Forces an update of workflow nodes and edges, bypassing EF Core's optimistic concurrency checks
        /// by using direct SQL commands instead of the regular EF Core change tracking.
        /// </summary>
        public async Task ForceUpdateNodesAndEdgesAsync(
            Guid workflowId, 
            List<WorkflowNodeDto> nodes, 
            List<WorkflowEdgeDto> edges, 
            CancellationToken cancellationToken = default)
        {
            // Detach any existing tracked entities for this workflow to prevent conflicts
            var trackedWorkflow = _dbContext.ChangeTracker.Entries<DDD.Entities.Workflow>()
                .FirstOrDefault(e => e.Entity.Id == workflowId);
                
            if (trackedWorkflow != null)
            {
                trackedWorkflow.State = EntityState.Detached;
            }
                
            var trackedNodes = _dbContext.ChangeTracker.Entries<WorkflowNode>()
                .Where(e => e.Entity.WorkflowId == workflowId)
                .ToList();
                
            foreach (var entry in trackedNodes)
            {
                entry.State = EntityState.Detached;
            }
                
            var trackedEdges = _dbContext.ChangeTracker.Entries<WorkflowEdge>()
                .Where(e => e.Entity.WorkflowId == workflowId)
                .ToList();
                
            foreach (var entry in trackedEdges)
            {
                entry.State = EntityState.Detached;
            }

            // First, get existing nodes and edges from database directly
            var existingNodes = await _dbContext.WorkflowNodes
                .AsNoTracking()
                .Where(n => n.WorkflowId == workflowId)
                .ToListAsync(cancellationToken);
                
            var existingEdges = await _dbContext.WorkflowEdges
                .AsNoTracking()
                .Where(e => e.WorkflowId == workflowId)
                .ToListAsync(cancellationToken);
            
            // Determine which nodes to add, update, or delete
            var nodesToDelete = existingNodes
                .Where(n => !nodes.Any(dto => dto.Id == n.Id))
                .ToList();
                
            var nodesToUpdate = existingNodes
                .Where(n => nodes.Any(dto => dto.Id == n.Id))
                .ToList();
                
            var nodesToAdd = nodes
                .Where(dto => !existingNodes.Any(n => n.Id == dto.Id))
                .ToList();
                
            // Similarly for edges
            var edgesToDelete = existingEdges
                .Where(e => !edges.Any(dto => dto.Id == e.Id))
                .ToList();
                
            var edgesToUpdate = existingEdges
                .Where(e => edges.Any(dto => dto.Id == e.Id))
                .ToList();
                
            var edgesToAdd = edges
                .Where(dto => !existingEdges.Any(e => e.Id == dto.Id))
                .ToList();
                
            // Create a temporary ID mapping dictionary for new nodes
            Dictionary<Guid, Guid> nodeIdMap = new Dictionary<Guid, Guid>();
            
            // Delete nodes first (due to foreign key constraints)
            foreach (var node in nodesToDelete)
            {
                _dbContext.WorkflowNodes.Remove(node);
            }
            
            // Delete edges that need to be removed
            foreach (var edge in edgesToDelete)
            {
                _dbContext.WorkflowEdges.Remove(edge);
            }
            
            // Update existing nodes
            foreach (var existingNode in nodesToUpdate)
            {
                var nodeDto = nodes.First(dto => dto.Id == existingNode.Id);
                
                // Update the node properties
                existingNode.Update(
                    nodeDto.NodeType,
                    nodeDto.NodeKind,
                    nodeDto.X,
                    nodeDto.Y,
                    nodeDto.Label,
                    nodeDto.ConfigJson
                );
                
                _dbContext.WorkflowNodes.Update(existingNode);
            }
            
            // Add new nodes
            foreach (var nodeDto in nodesToAdd)
            {
                var newNode = WorkflowNode.Create(
                    workflowId,
                    nodeDto.NodeType,
                    nodeDto.NodeKind,
                    nodeDto.X,
                    nodeDto.Y,
                    nodeDto.Label,
                    nodeDto.ConfigJson
                );
                
                // Store mapping from temporary ID to real ID
                nodeIdMap[nodeDto.Id] = newNode.Id;
                
                await _dbContext.WorkflowNodes.AddAsync(newNode, cancellationToken);
            }
            
            // We need to save changes to get the real IDs for new nodes
            // before processing edges that might reference them
            await _dbContext.SaveChangesAsync(cancellationToken);
            
            // Update existing edges
            foreach (var existingEdge in edgesToUpdate)
            {
                var edgeDto = edges.First(dto => dto.Id == existingEdge.Id);
                
                // Update the edge properties
                existingEdge.Update(
                    edgeDto.Label,
                    edgeDto.EdgeType,
                    edgeDto.ConditionJson,
                    edgeDto.SourceHandle,
                    edgeDto.TargetHandle
                );
                
                _dbContext.WorkflowEdges.Update(existingEdge);
            }
            
            // Add new edges
            foreach (var edgeDto in edgesToAdd)
            {
                // Resolve source and target node IDs (they might be temporary IDs)
                Guid sourceNodeId = nodeIdMap.ContainsKey(edgeDto.SourceNodeId) 
                    ? nodeIdMap[edgeDto.SourceNodeId] 
                    : edgeDto.SourceNodeId;
                    
                Guid targetNodeId = nodeIdMap.ContainsKey(edgeDto.TargetNodeId) 
                    ? nodeIdMap[edgeDto.TargetNodeId] 
                    : edgeDto.TargetNodeId;
                
                var newEdge = WorkflowEdge.Create(
                    workflowId,
                    sourceNodeId,
                    targetNodeId,
                    edgeDto.Label,
                    edgeDto.EdgeType,
                    edgeDto.ConditionJson,
                    edgeDto.SourceHandle,
                    edgeDto.TargetHandle
                );
                
                await _dbContext.WorkflowEdges.AddAsync(newEdge, cancellationToken);
            }
            
            // Save all changes
            await _dbContext.SaveChangesAsync(cancellationToken);
            
            // Update the workflow Version property to reset concurrency token
            // Use a direct ExecuteSql approach to bypass EF concurrency validation
            await _dbContext.Database.ExecuteSqlRawAsync(
                "UPDATE workflow.\"Workflows\" SET \"Version\" = @p0 WHERE \"Id\" = @p1",
                new byte[0], workflowId);
        }

        private List<IDomainEvent> CollectDomainEvents()
        {
            // Get all entities with domain events
            var entitiesWithEvents = _dbContext.ChangeTracker.Entries<Aggregate<Guid>>()
                .Select(e => e.Entity)
                .Where(e => e.DomainEvents.Any())
                .ToList();

            // Get all domain events
            var domainEvents = entitiesWithEvents
                .SelectMany(e => e.DomainEvents)
                .ToList();

            // Clear domain events from entities
            entitiesWithEvents.ForEach(e => e.ClearDomainEvents());

            return domainEvents;
        }
    }
}
