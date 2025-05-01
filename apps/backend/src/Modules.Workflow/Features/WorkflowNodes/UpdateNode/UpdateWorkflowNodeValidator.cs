using FluentValidation;

namespace Modules.Workflow.Features.WorkflowNodes.UpdateNode
{
    public class UpdateWorkflowNodeValidator : AbstractValidator<UpdateWorkflowNodeCommand>
    {
        public UpdateWorkflowNodeValidator()
        {
            RuleFor(x => x.NodeId)
                .NotEmpty()
                .WithMessage("Node ID is required.");

            RuleFor(x => x.WorkflowId)
                .NotEmpty()
                .WithMessage("Workflow ID is required.");

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