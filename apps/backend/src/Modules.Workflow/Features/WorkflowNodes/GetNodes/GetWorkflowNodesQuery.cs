using MediatR;
using Modules.Workflow.Features.Workflows.Common;

namespace Modules.Workflow.Features.WorkflowNodes.GetNodes
{
    public record GetWorkflowNodesQuery(Guid WorkflowId) : IRequest<IEnumerable<WorkflowNodeResponse>?>;
}