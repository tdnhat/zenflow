using MediatR;

namespace Modules.Workflow.Features.Workflows.ValidateWorkflow
{
    public record ValidateWorkflowCommand(Guid WorkflowId) : IRequest<ValidateWorkflowResponse>;
}