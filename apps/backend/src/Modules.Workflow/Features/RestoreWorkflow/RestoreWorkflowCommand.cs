using MediatR;
using Modules.Workflow.Dtos;

namespace Modules.Workflow.Features.RestoreWorkflow
{
    public record RestoreWorkflowCommand(Guid Id) : IRequest<WorkflowDto?>;
}