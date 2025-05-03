using MediatR;
using Modules.Workflow.Dtos;

namespace Modules.Workflow.Features.WorkflowEdges.UpdateEdge
{
    public class UpdateWorkflowEdgeCommand : IRequest<WorkflowEdgeDto?>
    {
        public Guid EdgeId { get; set; }
        public Guid WorkflowId { get; set; }
        public string Label { get; set; } = string.Empty;
        public string EdgeType { get; set; } = "default";
        public string ConditionJson { get; set; } = "{}";
        public string SourceHandle { get; set; } = string.Empty;
        public string TargetHandle { get; set; } = string.Empty;
    }
}
