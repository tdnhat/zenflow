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

            // Generate the workflow ID once
            var workflowId = Guid.NewGuid();

            // Create a dictionary to map the client-side node IDs to server-side node IDs
            var nodeIdMappings = new Dictionary<Guid, Guid>();
            
            // First, create nodes with new server-side IDs but keep track of the mapping
            var nodes = request.Nodes.Select(n => 
            {
                // Generate a new ID for each node and maintain a mapping
                var newNodeId = Guid.NewGuid();
                nodeIdMappings[n.Id] = newNodeId;
                
                return new WorkflowNode
                {
                    Id = newNodeId,
                    WorkflowId = workflowId,
                    Name = n.Name,
                    ActivityType = n.ActivityType,
                    ActivityPropertiesJson = JsonSerializer.Serialize(n.ActivityProperties),
                    ActivityProperties = n.ActivityProperties,
                    InputMappingsJson = JsonSerializer.Serialize(n.InputMappings.Select(m => new InputMapping
                    {
                        // Map the source node ID if it exists in our mappings
                        SourceNodeId = nodeIdMappings.ContainsKey(m.SourceNodeId) ? 
                            nodeIdMappings[m.SourceNodeId] : m.SourceNodeId,
                        SourceProperty = m.SourceProperty,
                        TargetProperty = m.TargetProperty
                    })),
                    InputMappings = n.InputMappings.Select(m => new InputMapping
                    {
                        // Map the source node ID if it exists in our mappings
                        SourceNodeId = nodeIdMappings.ContainsKey(m.SourceNodeId) ? 
                            nodeIdMappings[m.SourceNodeId] : m.SourceNodeId,
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
                };
            }).ToList();

            // Map command to domain entity
            var workflow = new WorkflowDefinition
            {
                Id = workflowId,
                Name = request.Name,
                Description = request.Description,
                Version = 1,
                CreatedAt = DateTime.UtcNow,
                Nodes = nodes,
                Edges = request.Edges.Select(e => {
                    // Handle condition safely
                    EdgeCondition? edgeCondition = null;
                    string? conditionJson = null;
                    
                    if (e.Condition != null && !string.IsNullOrEmpty(e.Condition.Expression))
                    {
                        edgeCondition = new EdgeCondition { Expression = e.Condition.Expression };
                        conditionJson = JsonSerializer.Serialize(edgeCondition);
                    }
                    
                    return new WorkflowEdge
                    {
                        Id = Guid.NewGuid(),
                        WorkflowId = workflowId,
                        // Map the source and target IDs to our newly generated IDs
                        Source = nodeIdMappings[e.Source],
                        Target = nodeIdMappings[e.Target],
                        ConditionJson = conditionJson ?? string.Empty,
                        Condition = edgeCondition
                    };
                }).ToList()
            };

            // Save to repository
            var savedWorkflowId = await _workflowRepository.SaveAsync(workflow, cancellationToken);

            _logger.LogInformation("Successfully created workflow definition with ID {WorkflowId}", savedWorkflowId);

            return savedWorkflowId;
        }
    }
}