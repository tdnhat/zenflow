using System.ComponentModel.DataAnnotations;
using MediatR;

namespace Modules.Workflow.Features.Workflows.RunWorkflow
{
    public record RunWorkflowCommand : IRequest<Guid>
    {
        [Required]
        public Guid WorkflowId { get; init; }
        
        public Dictionary<string, object> Variables { get; init; } = new();
    }
}