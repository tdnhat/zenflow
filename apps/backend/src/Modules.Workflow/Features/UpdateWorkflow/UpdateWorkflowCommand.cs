using MediatR;
using Modules.Workflow.Dtos;

namespace Modules.Workflow.Features.UpdateWorkflow
{
    public record UpdateWorkflowCommand(Guid Id, string Name, string Description) : IRequest<WorkflowDto?>;
}