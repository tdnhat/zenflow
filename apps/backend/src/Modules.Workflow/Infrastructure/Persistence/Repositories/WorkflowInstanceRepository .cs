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
        private readonly JsonSerializerOptions _jsonOptions;

        public WorkflowInstanceRepository(WorkflowDbContext dbContext)
        {
            _dbContext = dbContext;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };
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
                    Error = context.Error,
                    VariablesJson = JsonSerializer.Serialize(context.Variables, _jsonOptions),
                    NodeExecutions = new List<NodeExecution>()
                };

                await _dbContext.WorkflowInstances.AddAsync(entity, cancellationToken);
            }
            else
            {
                entity.Status = context.Status.ToString();
                entity.StartedAt = context.StartedAt;
                entity.CompletedAt = context.CompletedAt;
                entity.Error = context.Error;
                entity.VariablesJson = JsonSerializer.Serialize(context.Variables, _jsonOptions);

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
                    Error = nodeContext.Error,
                    InputDataJson = JsonSerializer.Serialize(nodeContext.InputData, _jsonOptions),
                    OutputDataJson = JsonSerializer.Serialize(nodeContext.OutputData, _jsonOptions),
                    LogsJson = JsonSerializer.Serialize(nodeContext.Logs, _jsonOptions)
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
                    : JsonSerializer.Deserialize<Dictionary<string, object>>(entity.VariablesJson, _jsonOptions),
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
                        : JsonSerializer.Deserialize<Dictionary<string, object>>(nodeExecution.InputDataJson, _jsonOptions),
                    OutputData = string.IsNullOrEmpty(nodeExecution.OutputDataJson)
                        ? new Dictionary<string, object>()
                        : JsonSerializer.Deserialize<Dictionary<string, object>>(nodeExecution.OutputDataJson, _jsonOptions),
                    Logs = string.IsNullOrEmpty(nodeExecution.LogsJson)
                        ? new List<string>()
                        : JsonSerializer.Deserialize<List<string>>(nodeExecution.LogsJson, _jsonOptions)
                };

                context.NodeExecutions[nodeExecution.NodeId] = nodeContext;
            }

            return context;
        }
    }
}