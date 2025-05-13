using MediatR;
using Modules.Workflow.Features.Workflows.Common;

namespace Modules.Workflow.Features.WorkflowNodes.GetNodeById
{
    public record GetWorkflowNodeByIdQuery(Guid WorkflowId, Guid NodeId) : IRequest<WorkflowNodeResponse?>;
}