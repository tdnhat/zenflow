using MediatR;
using Modules.Workflow.Dtos;

namespace Modules.Workflow.Features.Workflows.RestoreWorkflow
{
    public record RestoreWorkflowCommand(Guid Id) : IRequest<WorkflowDto?>;
}