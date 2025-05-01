using MediatR;
using Modules.Workflow.Dtos;

namespace Modules.Workflow.Features.WorkflowNodes.UpdateNode
{
    public record UpdateWorkflowNodeCommand(
        Guid NodeId,
        Guid WorkflowId,
        float X,
        float Y,
        string Label,
        string ConfigJson) : IRequest<WorkflowNodeDto?>;
}