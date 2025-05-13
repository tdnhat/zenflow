using MediatR;
using Modules.Workflow.Features.Workflows.Common;

namespace Modules.Workflow.Features.WorkflowNodes.UpdateNode
{
    public record UpdateWorkflowNodeCommand(
        Guid NodeId,
        Guid WorkflowId,
        float X,
        float Y,
        string Label,
        string ConfigJson) : IRequest<WorkflowNodeResponse?>;
}