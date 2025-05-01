using FluentValidation;

namespace Modules.User.Features.DeleteUserById
{
    public class DeleteUserByIdValidator : AbstractValidator<DeleteUserByIdCommand>
    {
        public DeleteUserByIdValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("User ID is required.");
        }
    }
}