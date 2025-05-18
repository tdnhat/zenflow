using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Modules.Workflow.Domain.Core;
using Modules.Workflow.Domain.Entities;
using Modules.Workflow.Domain.Enums;
using Modules.Workflow.Domain.Interfaces.Core;

namespace Modules.Workflow.Infrastructure.Persistence.Repositories
{
    public class WorkflowInstanceRepository : IWorkflowInstanceRepository
    {
        private readonly WorkflowDbContext _dbContext;
        
        public WorkflowInstanceRepository(WorkflowDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<WorkflowExecutionContext> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.WorkflowInstances
                .Include(w => w.NodeExecutions)
                .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);

            if (entity == null)
                return null;

            return MapToExecutionContext(entity);
        }

        public async Task<IEnumerable<WorkflowExecutionContext>> GetByWorkflowIdAsync(Guid workflowId, CancellationToken cancellationToken = default)
        {
            var entities = await _dbContext.WorkflowInstances
                .Include(w => w.NodeExecutions)
                .Where(w => w.WorkflowDefinitionId == workflowId)
                .ToListAsync(cancellationToken);

            return entities.Select(MapToExecutionContext);
        }

        public async Task SaveAsync(WorkflowExecutionContext context, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.WorkflowInstances
                .Include(w => w.NodeExecutions)
                .FirstOrDefaultAsync(w => w.Id == context.WorkflowInstanceId, cancellationToken);

            if (entity == null)
            {
                entity = new WorkflowInstance
                {
                    Id = context.WorkflowInstanceId,
                    WorkflowDefinitionId = context.WorkflowDefinitionId,
                    Status = context.Status.ToString(),
                    StartedAt = context.StartedAt,
                    CompletedAt = context.CompletedAt,
                    Error = context.Error ?? string.Empty,
                    VariablesJson = JsonSerializer.Serialize(context.Variables, 
                        new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }),
                    NodeExecutions = new List<NodeExecution>()
                };

                await _dbContext.WorkflowInstances.AddAsync(entity, cancellationToken);
            }
            else
            {
                entity.Status = context.Status.ToString();
                entity.StartedAt = context.StartedAt;
                entity.CompletedAt = context.CompletedAt;
                entity.Error = context.Error ?? string.Empty;
                entity.VariablesJson = JsonSerializer.Serialize(context.Variables, 
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

                // Remove existing node executions
                _dbContext.NodeExecutions.RemoveRange(entity.NodeExecutions);
            }

            // Add node executions
            foreach (var (nodeId, nodeContext) in context.NodeExecutions)
            {
                var nodeExecution = new NodeExecution
                {
                    Id = Guid.NewGuid(),
                    WorkflowInstanceId = context.WorkflowInstanceId,
                    NodeId = nodeId,
                    ActivityType = nodeContext.ActivityType,
                    Status = nodeContext.Status.ToString(),
                    StartedAt = nodeContext.StartedAt,
                    CompletedAt = nodeContext.CompletedAt,
                    Error = nodeContext.Error ?? string.Empty,
                    InputDataJson = JsonSerializer.Serialize(nodeContext.InputData, 
                        new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }),
                    OutputDataJson = JsonSerializer.Serialize(nodeContext.OutputData, 
                        new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }),
                    LogsJson = JsonSerializer.Serialize(nodeContext.Logs, 
                        new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })
                };

                await _dbContext.NodeExecutions.AddAsync(nodeExecution, cancellationToken);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.WorkflowInstances
                .Include(w => w.NodeExecutions)
                .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);

            if (entity != null)
            {
                _dbContext.NodeExecutions.RemoveRange(entity.NodeExecutions);
                _dbContext.WorkflowInstances.Remove(entity);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        private WorkflowExecutionContext MapToExecutionContext(WorkflowInstance entity)
        {
            var context = new WorkflowExecutionContext
            {
                WorkflowInstanceId = entity.Id,
                WorkflowDefinitionId = entity.WorkflowDefinitionId,
                Status = Enum.Parse<WorkflowStatus>(entity.Status),
                StartedAt = entity.StartedAt,
                CompletedAt = entity.CompletedAt,
                Error = entity.Error,
                Variables = string.IsNullOrEmpty(entity.VariablesJson)
                    ? new Dictionary<string, object>()
                    : JsonSerializer.Deserialize<Dictionary<string, object>>(entity.VariablesJson, 
                        new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }),
                NodeExecutions = new Dictionary<string, NodeExecutionContext>()
            };

            foreach (var nodeExecution in entity.NodeExecutions)
            {
                var nodeContext = new NodeExecutionContext
                {
                    NodeId = nodeExecution.NodeId,
                    ActivityType = nodeExecution.ActivityType,
                    Status = Enum.Parse<WorkflowNodeStatus>(nodeExecution.Status),
                    StartedAt = nodeExecution.StartedAt,
                    CompletedAt = nodeExecution.CompletedAt,
                    Error = nodeExecution.Error,
                    InputData = string.IsNullOrEmpty(nodeExecution.InputDataJson)
                        ? new Dictionary<string, object>()
                        : JsonSerializer.Deserialize<Dictionary<string, object>>(nodeExecution.InputDataJson, 
                            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }),
                    OutputData = string.IsNullOrEmpty(nodeExecution.OutputDataJson)
                        ? new Dictionary<string, object>()
                        : JsonSerializer.Deserialize<Dictionary<string, object>>(nodeExecution.OutputDataJson, 
                            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }),
                    Logs = string.IsNullOrEmpty(nodeExecution.LogsJson)
                        ? new List<string>()
                        : JsonSerializer.Deserialize<List<string>>(nodeExecution.LogsJson, 
                            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })
                };

                context.NodeExecutions[nodeExecution.NodeId] = nodeContext;
            }

            return context;
        }
    }
}