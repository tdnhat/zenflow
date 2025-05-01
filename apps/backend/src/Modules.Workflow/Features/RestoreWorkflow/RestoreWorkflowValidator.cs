using FluentValidation;

namespace Modules.Workflow.Features.RestoreWorkflow
{
    public class RestoreWorkflowValidator : AbstractValidator<RestoreWorkflowCommand>
    {
        public RestoreWorkflowValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Workflow ID is required.");
        }
    }
}