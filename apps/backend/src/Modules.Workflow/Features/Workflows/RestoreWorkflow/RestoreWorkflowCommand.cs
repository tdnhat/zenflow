using MediatR;
using Modules.Workflow.Features.Workflows.Common;

namespace Modules.Workflow.Features.Workflows.RestoreWorkflow
{
    public record RestoreWorkflowCommand(Guid Id) : IRequest<WorkflowResponse?>;
}