using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace Modules.Workflow.Features.Workflows.UpdateWorkflowDefinition
{
    public record UpdateWorkflowDefinitionCommand : IRequest<Guid>
    {
        [Required]
        public Guid WorkflowId { get; init; }

        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Name { get; init; }

        [StringLength(500)]
        public string Description { get; init; }

        [Required]
        public List<UpdateWorkflowNodeDto> Nodes { get; init; } = new();

        [Required]
        public List<UpdateWorkflowEdgeDto> Edges { get; init; } = new();
    }

    public class UpdateWorkflowNodeDto
    {
        [Required]
        public string Id { get; init; }

        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Name { get; init; }

        [Required]
        [StringLength(200)]
        public string ActivityType { get; init; }

        public Dictionary<string, object> ActivityProperties { get; init; } = new();

        public List<UpdateInputMappingDto> InputMappings { get; init; } = new();

        public List<UpdateOutputMappingDto> OutputMappings { get; init; } = new();

        [Required]
        public UpdateNodePositionDto Position { get; init; } = new();
    }

    public class UpdateWorkflowEdgeDto
    {
        [Required]
        public string Id { get; init; }

        [Required]
        public string Source { get; init; }

        [Required]
        public string Target { get; init; }

        public UpdateEdgeConditionDto Condition { get; init; }
    }

    public class UpdateInputMappingDto
    {
        [Required]
        public string SourceNodeId { get; init; }

        [Required]
        public string SourceProperty { get; init; }

        [Required]
        public string TargetProperty { get; init; }
    }

    public class UpdateOutputMappingDto
    {
        [Required]
        public string SourceProperty { get; init; }

        [Required]
        public string TargetProperty { get; init; }
    }

    public class UpdateEdgeConditionDto
    {
        public string Expression { get; init; }
    }

    public class UpdateNodePositionDto
    {
        public int X { get; init; }
        public int Y { get; init; }
    }
}