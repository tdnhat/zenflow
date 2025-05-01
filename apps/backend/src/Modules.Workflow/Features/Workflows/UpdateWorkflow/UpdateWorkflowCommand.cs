using MediatR;
using Modules.Workflow.Dtos;

namespace Modules.Workflow.Features.Workflows.UpdateWorkflow
{
    public record UpdateWorkflowCommand(Guid Id, string Name, string Description) : IRequest<WorkflowDto?>;
}