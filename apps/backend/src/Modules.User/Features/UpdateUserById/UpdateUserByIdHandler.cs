using MediatR;
using Microsoft.Extensions.Logging;
using Modules.User.DDD.Interfaces;
using Modules.User.Dtos;

namespace Modules.User.Features.UpdateUserById;

public class UpdateUserByIdHandler : IRequestHandler<UpdateUserByIdCommand, UserDto?>
{
    private readonly IKeycloakAdminService _keycloak;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UpdateUserByIdHandler> _logger;

    public UpdateUserByIdHandler(
        IKeycloakAdminService keycloak,
        IUserRepository userRepository,
        ILogger<UpdateUserByIdHandler> logger)
    {
        _keycloak = keycloak;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<UserDto?> Handle(UpdateUserByIdCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);
        if (user == null)
        {
            _logger.LogWarning("User with ID {UserId} not found", request.Id);
            return null;
        }

        _logger.LogInformation("Updating user {UserId} (externalId: {ExternalId})", user.Id, user.ExternalId);

        try
        {
            // App: update username (and sync to Keycloak)
            if (!string.IsNullOrWhiteSpace(request.Username) && user.Username != request.Username)
            {
                user.Update(request.Username);
                await _keycloak.UpdateUserUsernameAsync(user.ExternalId, request.Username);
            }

            // Keycloak: update roles
            if (request.Roles?.Count > 0)
            {
                await _keycloak.UpdateUserRolesAsync(user.ExternalId, request.Roles);
            }

            // Keycloak: enable/disable user
            if (request.IsActive.HasValue)
            {
                await _keycloak.SetUserActiveStatusAsync(user.ExternalId, request.IsActive.Value);
            }

            await _userRepository.UpdateAsync(user, cancellationToken);

            return new UserDto(user.Id, user.Username, user.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update user {UserId}", request.Id);
            throw;
        }
    }
}
