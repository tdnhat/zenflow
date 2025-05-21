using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;
using Modules.Workflow.Domain.Interfaces.Core;
using Modules.Workflow.Domain.Entities;

namespace Modules.Workflow.Features.Workflows.UpdateWorkflowDefinition
{
    public class UpdateWorkflowDefinitionHandler : IRequestHandler<UpdateWorkflowDefinitionCommand, Guid>
    {
        private readonly IWorkflowRepository _workflowRepository;
        private readonly ILogger<UpdateWorkflowDefinitionHandler> _logger;

        public UpdateWorkflowDefinitionHandler(
            IWorkflowRepository workflowRepository,
            ILogger<UpdateWorkflowDefinitionHandler> logger)
        {
            _workflowRepository = workflowRepository;
            _logger = logger;
        }

        public async Task<Guid> Handle(UpdateWorkflowDefinitionCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating workflow definition with ID {WorkflowId}", request.WorkflowId);

            // Check if workflow exists
            var existingWorkflow = await _workflowRepository.GetByIdAsync(request.WorkflowId, cancellationToken);
            if (existingWorkflow == null)
            {
                _logger.LogWarning("Workflow definition with ID {WorkflowId} not found", request.WorkflowId);
                throw new KeyNotFoundException($"Workflow definition with ID {request.WorkflowId} not found");
            }

            // Map command to domain entity
            var workflow = new WorkflowDefinition
            {
                Id = request.WorkflowId,
                Name = request.Name,
                Description = request.Description,
                Version = existingWorkflow.Version + 1,
                CreatedAt = existingWorkflow.CreatedAt,
                UpdatedAt = DateTime.UtcNow,
                Nodes = request.Nodes?.Select(n => new WorkflowNode
                {
                    Id = n.Id,
                    WorkflowId = request.WorkflowId,
                    Name = n.Name,
                    ActivityType = n.ActivityType,
                    ActivityPropertiesJson = JsonSerializer.Serialize(n.ActivityProperties),
                    ActivityProperties = n.ActivityProperties,
                    InputMappingsJson = JsonSerializer.Serialize(n.InputMappings.Select(m => new InputMapping
                    {
                        SourceNodeId = m.SourceNodeId,
                        SourceProperty = m.SourceProperty,
                        TargetProperty = m.TargetProperty
                    })),
                    InputMappings = n.InputMappings.Select(m => new InputMapping
                    {
                        SourceNodeId = m.SourceNodeId,
                        SourceProperty = m.SourceProperty,
                        TargetProperty = m.TargetProperty
                    }).ToList(),
                    OutputMappingsJson = JsonSerializer.Serialize(n.OutputMappings.Select(m => new OutputMapping
                    {
                        SourceProperty = m.SourceProperty,
                        TargetProperty = m.TargetProperty
                    })),
                    OutputMappings = n.OutputMappings.Select(m => new OutputMapping
                    {
                        SourceProperty = m.SourceProperty,
                        TargetProperty = m.TargetProperty
                    }).ToList(),
                    PositionJson = JsonSerializer.Serialize(new NodePosition
                    {
                        X = n.Position.X,
                        Y = n.Position.Y
                    }),
                    Position = new NodePosition
                    {
                        X = n.Position.X,
                        Y = n.Position.Y
                    }
                }).ToList() ?? new List<WorkflowNode>(),
                Edges = request.Edges?.Select(e => new WorkflowEdge
                {
                    Id = e.Id,
                    WorkflowId = request.WorkflowId,
                    Source = e.Source,
                    Target = e.Target,
                    ConditionJson = e.Condition != null ? JsonSerializer.Serialize(new EdgeCondition
                    {
                        Expression = e.Condition.Expression
                    }) : null,
                    Condition = e.Condition != null ? new EdgeCondition
                    {
                        Expression = e.Condition.Expression
                    } : null
                }).ToList() ?? new List<WorkflowEdge>()
            };

            // Determine if only metadata (name/description) is being updated
            bool onlyMetadataUpdate = (request.Nodes == null || !request.Nodes.Any()) && 
                                      (request.Edges == null || !request.Edges.Any()) &&
                                      (existingWorkflow.Name != request.Name || existingWorkflow.Description != request.Description);

            // If nodes and edges are not provided, but name/description might change, preserve existing nodes/edges.
            // If nodes/edges ARE provided (even if empty), they will overwrite existing ones.
            if (request.Nodes == null)
            {
                workflow.Nodes = existingWorkflow.Nodes;
            }
            if (request.Edges == null)
            {
                workflow.Edges = existingWorkflow.Edges;
            }

            // Validate data integrity before saving
            var finalNodeIds = new HashSet<Guid>(workflow.Nodes.Select(n => n.Id));
            foreach (var edge in workflow.Edges)
            {
                if (!finalNodeIds.Contains(edge.Source))
                {
                    var errorMessage = $"Edge with ID '{edge.Id}' references a source node (ID: '{edge.Source}') that is not present in the final set of nodes for workflow '{request.WorkflowId}'.";
                    _logger.LogWarning(errorMessage + " Ensure the source node exists or is included in the update.");
                    throw new InvalidOperationException(errorMessage);
                }
                if (!finalNodeIds.Contains(edge.Target))
                {
                    var errorMessage = $"Edge with ID '{edge.Id}' references a target node (ID: '{edge.Target}') that is not present in the final set of nodes for workflow '{request.WorkflowId}'.";
                    _logger.LogWarning(errorMessage + " Ensure the target node exists or is included in the update.");
                    throw new InvalidOperationException(errorMessage);
                }
            }

            // Save to repository
            var workflowId = await _workflowRepository.SaveAsync(workflow, cancellationToken);

            _logger.LogInformation("Successfully updated workflow definition with ID {WorkflowId}", workflowId);

            return workflowId;
        }
    }
}