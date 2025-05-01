using MediatR;
using Modules.Workflow.Dtos;

namespace Modules.Workflow.Features.WorkflowNodes.GetNodeById
{
    public record GetWorkflowNodeByIdQuery(Guid WorkflowId, Guid NodeId) : IRequest<WorkflowNodeDto?>;
}