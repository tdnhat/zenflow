using FluentValidation;

namespace Modules.User.Features.RestoreUserById
{
    public class RestoreUserByIdValidator : AbstractValidator<RestoreUserByIdCommand>
    {
        public RestoreUserByIdValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("User ID is required.");
        }
    }
}