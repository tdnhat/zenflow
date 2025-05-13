using MediatR;
using Modules.Workflow.Features.Workflows.Common;

namespace Modules.Workflow.Features.Workflows.ArchiveWorkflow
{
    public record ArchiveWorkflowCommand(Guid Id) : IRequest<WorkflowResponse?>;
}