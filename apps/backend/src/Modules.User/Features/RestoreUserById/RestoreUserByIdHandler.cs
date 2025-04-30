using MediatR;
using Microsoft.Extensions.Logging;
using Modules.User.DDD.Interfaces;
using Modules.User.Infrastructure.Exceptions;

namespace Modules.User.Features.RestoreUserById
{
    public class RestoreUserByIdHandler : IRequestHandler<RestoreUserByIdCommand>
    {
        private readonly IUserRepository _userRepository;
        private readonly IKeycloakAdminService _keycloak;
        private readonly ILogger<RestoreUserByIdHandler> _logger;
        public RestoreUserByIdHandler(
            IUserRepository userRepository,
            IKeycloakAdminService keycloak,
            ILogger<RestoreUserByIdHandler> logger)
        {
            _userRepository = userRepository;
            _keycloak = keycloak;
            _logger = logger;
        }
        public async Task Handle(RestoreUserByIdCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdIncludingDeletedAsync(request.Id, cancellationToken);

            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found", request.Id);
                throw new NotFoundException($"User with ID {request.Id} not found");
            }

            if (user.IsDeleted == false)
            {
                _logger.LogWarning("User with ID {UserId} is not deleted", request.Id);
                throw new InvalidOperationException($"User with ID {request.Id} is not deleted");
            }

            try
            {
                // Restore user in app DB
                await _userRepository.RestoreAsync(user.Id, cancellationToken);

                // Reactivate user in Keycloak
                await _keycloak.SetUserActiveStatusAsync(user.ExternalId, true);

                _logger.LogInformation("User {UserId} restored", user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring user {UserId}", request.Id);
                throw new Exception($"Error restoring user with ID {request.Id}", ex);
            }
        }
    }
}
