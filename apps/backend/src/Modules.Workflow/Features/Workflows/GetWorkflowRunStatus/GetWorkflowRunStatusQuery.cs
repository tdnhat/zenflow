using MediatR;
using Modules.Workflow.Features.Workflows.Shared;

namespace Modules.Workflow.Features.Workflows.GetWorkflowRunStatus
{
    public record GetWorkflowRunStatusQuery(Guid WorkflowRunId) : IRequest<WorkflowRunStatusDto?>;
}