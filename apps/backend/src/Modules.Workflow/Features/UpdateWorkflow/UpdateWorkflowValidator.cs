using FluentValidation;

namespace Modules.Workflow.Features.UpdateWorkflow
{
    public class UpdateWorkflowValidator : AbstractValidator<UpdateWorkflowCommand>
    {
        public UpdateWorkflowValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Workflow ID is required.");

            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Name is required.")
                .MaximumLength(100)
                .WithMessage("Name must not exceed 100 characters.");

            RuleFor(x => x.Description)
                .MaximumLength(255)
                .WithMessage("Description must not exceed 255 characters.");
        }
    }
}