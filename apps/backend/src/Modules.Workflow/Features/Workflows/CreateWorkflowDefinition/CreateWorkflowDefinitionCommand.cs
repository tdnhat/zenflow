using System.ComponentModel.DataAnnotations;
using MediatR;

namespace Modules.Workflow.Features.Workflows.CreateWorkflowDefinition
{
    public record CreateWorkflowDefinitionCommand : IRequest<Guid>
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Name { get; init; }

        [StringLength(500)]
        public string Description { get; init; }

        [Required]
        public List<CreateWorkflowNodeDto> Nodes { get; init; } = new();

        [Required]
        public List<CreateWorkflowEdgeDto> Edges { get; init; } = new();
    }

    public class CreateWorkflowNodeDto
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

        public List<CreateInputMappingDto> InputMappings { get; init; } = new();

        public List<CreateOutputMappingDto> OutputMappings { get; init; } = new();

        [Required]
        public CreateNodePositionDto Position { get; init; } = new();
    }

    public class CreateWorkflowEdgeDto
    {
        [Required]
        public string Id { get; init; }

        [Required]
        public string Source { get; init; }

        [Required]
        public string Target { get; init; }

        public CreateEdgeConditionDto Condition { get; init; }
    }

    public class CreateInputMappingDto
    {
        [Required]
        public string SourceNodeId { get; init; }

        [Required]
        public string SourceProperty { get; init; }

        [Required]
        public string TargetProperty { get; init; }
    }

    public class CreateOutputMappingDto
    {
        [Required]
        public string SourceProperty { get; init; }

        [Required]
        public string TargetProperty { get; init; }
    }

    public class CreateEdgeConditionDto
    {
        public string Expression { get; init; }
    }

    public class CreateNodePositionDto
    {
        public int X { get; init; }
        public int Y { get; init; }
    }
}