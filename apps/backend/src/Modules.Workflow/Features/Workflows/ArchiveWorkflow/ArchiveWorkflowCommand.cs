using MediatR;
using Modules.Workflow.Dtos;

namespace Modules.Workflow.Features.Workflows.ArchiveWorkflow
{
    public record ArchiveWorkflowCommand(Guid Id) : IRequest<WorkflowDto?>;
}