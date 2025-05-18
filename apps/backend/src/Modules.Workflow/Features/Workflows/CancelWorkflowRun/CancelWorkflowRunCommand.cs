using MediatR;

namespace Modules.Workflow.Features.Workflows.CancelWorkflowRun
{
    public record CancelWorkflowRunCommand(Guid WorkflowRunId) : IRequest;
}