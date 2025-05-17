using MediatR;
using Microsoft.Extensions.Logging;
using Modules.Workflow.Domain.Interfaces.Core;
using Modules.Workflow.Features.Workflows.Shared;

namespace Modules.Workflow.Features.Workflows.GetWorkflowById
{
    public class GetWorkflowDefinitionHandler : IRequestHandler<GetWorkflowDefinitionQuery, WorkflowDefinitionDto?>
    {
        private readonly IWorkflowRepository _workflowRepository;
        private readonly ILogger<GetWorkflowDefinitionHandler> _logger;

        public GetWorkflowDefinitionHandler(
            IWorkflowRepository workflowRepository,
            ILogger<GetWorkflowDefinitionHandler> logger
        )
        {
            _workflowRepository = workflowRepository;
            _logger = logger;
        }

        public async Task<WorkflowDefinitionDto?> Handle(GetWorkflowDefinitionQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting workflow definition with ID {WorkflowId}", request.Id);

            var workflow = await _workflowRepository.GetByIdAsync(request.Id, cancellationToken);

            if (workflow == null)
            {
                _logger.LogWarning("Workflow definition with ID {WorkflowId} not found", request.Id);
                return null;
            }

            // Map domain entity to DTO
            var result = new WorkflowDefinitionDto
            {
                Id = workflow.Id,
                Name = workflow.Name,
                Description = workflow.Description,
                Version = workflow.Version,
                CreatedAt = workflow.CreatedAt,
                UpdatedAt = workflow.UpdatedAt,
                Nodes = workflow.Nodes.Select(n => new WorkflowNodeDto
                {
                    Id = n.Id,
                    Name = n.Name,
                    ActivityType = n.ActivityType,
                    ActivityProperties = n.ActivityProperties,
                    Position = new NodePositionDto
                    {
                        X = n.Position.X,
                        Y = n.Position.Y
                    },
                    InputMappings = n.InputMappings.Select(m => new InputMappingDto
                    {
                        SourceNodeId = m.SourceNodeId,
                        SourceProperty = m.SourceProperty,
                        TargetProperty = m.TargetProperty
                    }).ToList(),
                    OutputMappings = n.OutputMappings.Select(m => new OutputMappingDto
                    {
                        SourceProperty = m.SourceProperty,
                        TargetProperty = m.TargetProperty
                    }).ToList()
                }).ToList(),
                Edges = workflow.Edges.Select(e => new WorkflowEdgeDto
                {
                    Id = e.Id,
                    Source = e.Source,
                    Target = e.Target,
                    Condition = e.Condition != null ? new EdgeConditionDto
                    {
                        Expression = e.Condition.Expression
                    } : null
                }).ToList()
            };

            return result;
        }
    }
}