using Microsoft.EntityFrameworkCore;
using Modules.Workflow.Domain.Interfaces.Core;
using Modules.Workflow.Domain.Entities;
using Modules.Workflow.Infrastructure.Services.Workflow.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Modules.Workflow.Infrastructure.Persistence.Repositories
{
    public class WorkflowRepository : IWorkflowRepository
    {
        private readonly WorkflowDbContext _dbContext;
        private readonly IWorkflowJsonLoader _workflowJsonLoader;

        public WorkflowRepository(WorkflowDbContext dbContext, IWorkflowJsonLoader workflowJsonLoader)
        {
            _dbContext = dbContext;
            _workflowJsonLoader = workflowJsonLoader;
        }

        public async Task<WorkflowDefinition> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.WorkflowDefinitions
                .Include(w => w.Nodes)
                .Include(w => w.Edges)
                .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);

            if (entity != null)
            {
                // Deserialize JSON properties for each node
                foreach (var node in entity.Nodes)
                {
                    DeserializeNodeProperties(node);
                }

                // Deserialize JSON properties for each edge
                foreach (var edge in entity.Edges)
                {
                    if (!string.IsNullOrEmpty(edge.ConditionJson))
                    {
                        edge.Condition = JsonSerializer.Deserialize<EdgeCondition>(
                            edge.ConditionJson, 
                            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                    }
                }
            }

            return entity;
        }

        public async Task<IEnumerable<WorkflowDefinition>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var entities = await _dbContext.WorkflowDefinitions
                .Include(w => w.Nodes)
                .Include(w => w.Edges)
                .ToListAsync(cancellationToken);

            // Deserialize JSON properties for all workflows
            foreach (var workflow in entities)
            {
                foreach (var node in workflow.Nodes)
                {
                    DeserializeNodeProperties(node);
                }

                foreach (var edge in workflow.Edges)
                {
                    if (!string.IsNullOrEmpty(edge.ConditionJson))
                    {
                        edge.Condition = JsonSerializer.Deserialize<EdgeCondition>(
                            edge.ConditionJson, 
                            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                    }
                }
            }

            return entities;
        }

        private void DeserializeNodeProperties(WorkflowNode node)
        {
            try
            {
                // Log the raw ActivityPropertiesJson for debugging
                Console.WriteLine($"Deserializing properties for node {node.Id}: {node.ActivityPropertiesJson}");
                
                // Deserialize activity properties
                if (!string.IsNullOrEmpty(node.ActivityPropertiesJson))
                {
                    var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                    node.ActivityProperties = JsonSerializer.Deserialize<Dictionary<string, object>>(
                        node.ActivityPropertiesJson, options) ?? new Dictionary<string, object>();
                    
                    // Log the deserialized properties
                    foreach (var prop in node.ActivityProperties)
                    {
                        Console.WriteLine($"Deserialized property: {prop.Key} = {prop.Value} ({prop.Value?.GetType().Name ?? "null"})");
                    }
                }

                // Deserialize input mappings
                if (!string.IsNullOrEmpty(node.InputMappingsJson))
                {
                    var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                    node.InputMappings = JsonSerializer.Deserialize<List<InputMapping>>(
                        node.InputMappingsJson, options) ?? new List<InputMapping>();
                }

                // Deserialize output mappings
                if (!string.IsNullOrEmpty(node.OutputMappingsJson))
                {
                    var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                    node.OutputMappings = JsonSerializer.Deserialize<List<OutputMapping>>(
                        node.OutputMappingsJson, options) ?? new List<OutputMapping>();
                }

                // Deserialize position
                if (!string.IsNullOrEmpty(node.PositionJson))
                {
                    var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                    node.Position = JsonSerializer.Deserialize<NodePosition>(
                        node.PositionJson, options) ?? new NodePosition();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deserializing node properties: {ex.Message}");
                throw;
            }
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
                    
                    // Ensure JSON properties are serialized
                    SerializeNodeProperties(node);
                    
                    _dbContext.WorkflowNodes.Add(node);
                }

                foreach (var edge in workflow.Edges)
                {
                    edge.WorkflowId = workflow.Id;
                    
                    // Ensure condition is serialized
                    if (edge.Condition != null)
                    {
                        edge.ConditionJson = JsonSerializer.Serialize(
                            edge.Condition, 
                            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                    }
                    
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
                    
                    // Ensure JSON properties are serialized
                    SerializeNodeProperties(node);
                }

                foreach (var edge in workflow.Edges)
                {
                    edge.WorkflowId = workflow.Id;
                    
                    // Ensure condition is serialized
                    if (edge.Condition != null)
                    {
                        edge.ConditionJson = JsonSerializer.Serialize(
                            edge.Condition, 
                            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                    }
                }
                
                // Add the workflow and all its related entities
                _dbContext.WorkflowDefinitions.Add(workflow);
                
                // Save everything in a single transaction
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            return workflow.Id;
        }

        private void SerializeNodeProperties(WorkflowNode node)
        {
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            
            // Serialize activity properties
            if (node.ActivityProperties != null && node.ActivityProperties.Count > 0)
            {
                node.ActivityPropertiesJson = JsonSerializer.Serialize(node.ActivityProperties, options);
            }
            
            // Serialize input mappings
            if (node.InputMappings != null && node.InputMappings.Count > 0)
            {
                node.InputMappingsJson = JsonSerializer.Serialize(node.InputMappings, options);
            }
            
            // Serialize output mappings
            if (node.OutputMappings != null && node.OutputMappings.Count > 0)
            {
                node.OutputMappingsJson = JsonSerializer.Serialize(node.OutputMappings, options);
            }
            
            // Serialize position
            if (node.Position != null)
            {
                node.PositionJson = JsonSerializer.Serialize(node.Position, options);
            }
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