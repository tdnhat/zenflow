using FluentValidation;

namespace Modules.User.Features.UpdateUserById
{
    public class UpdateUserByIdValidator : AbstractValidator<UpdateUserByIdCommand>
    {
        public UpdateUserByIdValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("User ID is required.");

            RuleFor(x => x.Username)
                .MaximumLength(100)
                .When(x => x.Username != null)
                .WithMessage("Username must not exceed 100 characters.");

            RuleFor(x => x.Roles)
                .Must(roles => roles == null || roles.All(role => !string.IsNullOrWhiteSpace(role)))
                .WithMessage("Roles cannot contain empty values.");
        }
    }
}