using MediatR;

namespace Modules.Workflow.Features.WorkflowExecutions.CancelWorkflow
{
    public record CancelWorkflowCommand(string WorkflowId) : IRequest<CancelWorkflowResult>;
}
