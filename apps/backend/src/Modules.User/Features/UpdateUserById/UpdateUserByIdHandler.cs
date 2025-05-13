using MediatR;
using Microsoft.Extensions.Logging;
using Modules.User.Domain.Interfaces;
using Modules.User.Infrastructure.Exceptions;

namespace Modules.User.Features.UpdateUserById;

public class UpdateUserByIdHandler : IRequestHandler<UpdateUserByIdCommand, UpdateUserResponse?>
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

    public async Task<UpdateUserResponse?> Handle(UpdateUserByIdCommand request, CancellationToken cancellationToken)
    {
        // Find user in our database
        var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);
        if (user == null)
        {
            _logger.LogWarning("User with ID {UserId} not found", request.Id);
            return null;
        }

        _logger.LogInformation("Updating user {UserId} (externalId: {ExternalId})", user.Id, user.ExternalId);

        try
        {
            // 1. Update username (and sync to Keycloak)
            if (!string.IsNullOrWhiteSpace(request.Username) && user.Username != request.Username)
            {
                user.Update(request.Username);
                await _keycloak.UpdateUserUsernameAsync(user.ExternalId, request.Username);
            }

            // 2. Update roles in Keycloak if requested
            if (request.Roles?.Count > 0)
            {
                await _keycloak.UpdateUserRolesAsync(user.ExternalId, request.Roles);
            }

            // 3. Update user active status in Keycloak if requested
            if (request.IsActive.HasValue)
            {
                await _keycloak.SetUserActiveStatusAsync(user.ExternalId, request.IsActive.Value);
            }

            // Save changes to our database
            await _userRepository.UpdateAsync(user, cancellationToken);

            // Return updated user details
            return new UpdateUserResponse(user.Id, user.Username, user.Email);
        }
        catch (KeycloakApiException ex)
        {
            _logger.LogError(ex, "Failed to update user {UserId} in Keycloak: {Message}", request.Id, ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update user {UserId}", request.Id);
            throw;
        }
    }
}
