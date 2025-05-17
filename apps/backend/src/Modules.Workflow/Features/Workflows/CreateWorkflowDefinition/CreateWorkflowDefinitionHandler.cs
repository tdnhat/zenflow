using MediatR;
using Microsoft.Extensions.Logging;
using Modules.Workflow.Domain.Entities;
using Modules.Workflow.Domain.Interfaces.Core;
using System.Text.Json;

namespace Modules.Workflow.Features.Workflows.CreateWorkflowDefinition
{
    public class CreateWorkflowDefinitionHandler : IRequestHandler<CreateWorkflowDefinitionCommand, Guid>
    {
        private readonly IWorkflowRepository _workflowRepository;
        private readonly ILogger<CreateWorkflowDefinitionHandler> _logger;

        public CreateWorkflowDefinitionHandler(
            IWorkflowRepository workflowRepository,
            ILogger<CreateWorkflowDefinitionHandler> logger
        )
        {
            _workflowRepository = workflowRepository;
            _logger = logger;
        }

        public async Task<Guid> Handle(CreateWorkflowDefinitionCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating new workflow definition with name '{Name}'", request.Name);

            // Map command to domain entity
            var workflow = new WorkflowDefinition
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                Version = 1,
                CreatedAt = DateTime.UtcNow,
                Nodes = request.Nodes.Select(n => new WorkflowNode
                {
                    Id = n.Id,
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
                }).ToList(),
                Edges = request.Edges.Select(e => new WorkflowEdge
                {
                    Id = e.Id,
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
                }).ToList()
            };

            // Save to repository
            var workflowId = await _workflowRepository.SaveAsync(workflow, cancellationToken);

            _logger.LogInformation("Successfully created workflow definition with ID {WorkflowId}", workflowId);

            return workflowId;
        }
    }
}