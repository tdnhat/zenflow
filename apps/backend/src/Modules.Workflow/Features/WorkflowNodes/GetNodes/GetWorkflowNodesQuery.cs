using MediatR;
using Modules.Workflow.Dtos;

namespace Modules.Workflow.Features.WorkflowNodes.GetNodes
{
    public record GetWorkflowNodesQuery(Guid WorkflowId) : IRequest<IEnumerable<WorkflowNodeDto>?>;
}