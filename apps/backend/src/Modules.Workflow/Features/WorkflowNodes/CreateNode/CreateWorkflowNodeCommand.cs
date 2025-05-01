using MediatR;
using Modules.Workflow.Dtos;

namespace Modules.Workflow.Features.WorkflowNodes.CreateNode
{
    public record CreateWorkflowNodeCommand(
        Guid WorkflowId, 
        string NodeType, 
        float X, 
        float Y, 
        string Label, 
        string ConfigJson) : IRequest<WorkflowNodeDto>;
}