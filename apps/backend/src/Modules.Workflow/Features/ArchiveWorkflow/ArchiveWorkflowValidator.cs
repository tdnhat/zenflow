using FluentValidation;

namespace Modules.Workflow.Features.ArchiveWorkflow
{
    public class ArchiveWorkflowValidator : AbstractValidator<ArchiveWorkflowCommand>
    {
        public ArchiveWorkflowValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Workflow ID is required.");
        }
    }
}