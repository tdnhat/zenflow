using FluentValidation;

namespace Modules.User.Features.GetCurrentUser
{
    public class GetCurrentUserValidator : AbstractValidator<GetCurrentUserQuery>
    {
        public GetCurrentUserValidator()
        {
            RuleFor(x => x.ExternalId)
                .NotEmpty()
                .WithMessage("External ID is required.");

            RuleFor(x => x.Username)
                .NotEmpty()
                .WithMessage("Username is required.")
                .MaximumLength(100)
                .WithMessage("Username must not exceed 100 characters.");

            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("Email is required.")
                .EmailAddress()
                .WithMessage("Invalid email format.")
                .MaximumLength(255)
                .WithMessage("Email must not exceed 255 characters.");
        }
    }
}