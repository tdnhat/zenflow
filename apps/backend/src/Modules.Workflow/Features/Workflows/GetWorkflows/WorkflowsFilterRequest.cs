using ZenFlow.Shared.Application.Models;

namespace Modules.Workflow.Features.Workflows.GetWorkflows
{
    public class WorkflowsFilterRequest : PaginatedRequest
    {
        public string? Status { get; set; }
        public string? SearchTerm { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public bool IncludeArchived { get; set; } = false;
    }
}