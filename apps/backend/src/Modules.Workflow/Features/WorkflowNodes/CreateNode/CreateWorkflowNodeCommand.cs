using MediatR;

namespace Modules.Workflow.Features.WorkflowNodes.CreateNode
{
    public record CreateWorkflowNodeCommand(
        Guid WorkflowId, 
        string NodeType,
        string NodeKind,
        float X, 
        float Y, 
        string Label, 
        string ConfigJson) : IRequest<CreateWorkflowNodeResponse>;
}