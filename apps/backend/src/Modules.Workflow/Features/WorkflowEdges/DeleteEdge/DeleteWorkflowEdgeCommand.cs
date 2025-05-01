using MediatR;

namespace Modules.Workflow.Features.WorkflowEdges.DeleteEdge
{
    public record DeleteWorkflowEdgeCommand(Guid EdgeId, Guid WorkflowId) : IRequest<bool>;
}