namespace ZenFlow.Shared.Application.Auth;

public interface ICurrentUserService
{
    string? UserId { get; }
    string? Email { get; }
    string? Username { get; }
}
