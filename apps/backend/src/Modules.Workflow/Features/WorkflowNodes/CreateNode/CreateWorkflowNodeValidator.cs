using FluentValidation;

namespace Modules.Workflow.Features.WorkflowNodes.CreateNode
{
    public class CreateWorkflowNodeValidator : AbstractValidator<CreateWorkflowNodeCommand>
    {
        public CreateWorkflowNodeValidator()
        {
            RuleFor(x => x.WorkflowId)
                .NotEmpty()
                .WithMessage("Workflow ID is required.");

            RuleFor(x => x.NodeType)
                .NotEmpty()
                .WithMessage("Node type is required.")
                .MaximumLength(50)
                .WithMessage("Node type must not exceed 50 characters.");

            RuleFor(x => x.Label)
                .NotEmpty()
                .WithMessage("Label is required.")
                .MaximumLength(100)
                .WithMessage("Label must not exceed 100 characters.");

            RuleFor(x => x.ConfigJson)
                .NotNull()
                .WithMessage("Configuration JSON cannot be null.");
        }
    }
}