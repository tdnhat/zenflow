using MediatR;
using Modules.Workflow.Features.WorkflowExecutions.Common;

namespace Modules.Workflow.Features.WorkflowExecutions.GetWorkflowExecutions
{
    public record GetWorkflowExecutionsQuery(string WorkflowId) : IRequest<List<WorkflowExecutionResponse>>;
}
