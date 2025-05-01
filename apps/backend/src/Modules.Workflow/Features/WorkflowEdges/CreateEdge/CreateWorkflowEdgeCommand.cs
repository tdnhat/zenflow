using MediatR;
using Modules.Workflow.Dtos;

namespace Modules.Workflow.Features.WorkflowEdges.CreateEdge
{
    public record CreateWorkflowEdgeCommand(
        Guid WorkflowId,
        Guid SourceNodeId,
        Guid TargetNodeId,
        string Label,
        string EdgeType,
        string ConditionJson,
        string SourceHandle,
        string TargetHandle) : IRequest<WorkflowEdgeDto>;
}