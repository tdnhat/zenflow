namespace Modules.Workflow.Features.Workflows.ValidateWorkflow
{
    public class ValidateWorkflowResponse
    {
        public bool IsValid { get; set; }
        public List<ValidationError> ValidationErrors { get; set; } = new();
    }

    public class ValidationError
    {
        public string? NodeId { get; set; }
        public string? EdgeId { get; set; }
        public string ErrorCode { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public string Severity { get; set; } = "error"; // error, warning
    }
}