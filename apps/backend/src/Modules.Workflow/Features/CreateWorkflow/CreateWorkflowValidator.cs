using FluentValidation;

namespace Modules.Workflow.Features.CreateWorkflow
{
    public class CreateWorkflowValidator : AbstractValidator<CreateWorkflowCommand>
    {
        public CreateWorkflowValidator()
        {
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
