using MediatR;

namespace Modules.Workflow.Features.WorkflowNodes.DeleteNode
{
    public record DeleteWorkflowNodeCommand(Guid NodeId, Guid WorkflowId) : IRequest<bool>;
}