using FluentValidation;

namespace Modules.Workflow.Features.WorkflowNodes.DeleteNode
{
    public class DeleteWorkflowNodeValidator : AbstractValidator<DeleteWorkflowNodeCommand>
    {
        public DeleteWorkflowNodeValidator()
        {
            RuleFor(x => x.NodeId)
                .NotEmpty()
                .WithMessage("Node ID is required.");

            RuleFor(x => x.WorkflowId)
                .NotEmpty()
                .WithMessage("Workflow ID is required.");
        }
    }
}