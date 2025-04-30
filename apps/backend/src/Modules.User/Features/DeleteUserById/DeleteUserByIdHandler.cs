using MediatR;
using Microsoft.Extensions.Logging;
using Modules.User.DDD.Interfaces;

namespace Modules.User.Features.DeleteUserById
{
    internal class DeleteUserByIdHandler : IRequestHandler<DeleteUserByIdCommand>
    {
        private readonly IUserRepository _userRepository;
        private readonly IKeycloakAdminService _keycloak;
        private readonly ILogger<DeleteUserByIdHandler> _logger;
        public DeleteUserByIdHandler(
            IUserRepository userRepository,
            IKeycloakAdminService keycloak,
            ILogger<DeleteUserByIdHandler> logger)
        {
            _userRepository = userRepository;
            _keycloak = keycloak;
            _logger = logger;
        }
        public async Task Handle(DeleteUserByIdCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found", request.Id);
                return;
            }

            _logger.LogInformation("Deleting user {UserId} (externalId: {ExternalId})", user.Id, user.ExternalId);
            try
            {
                // Delete user in Keycloak
                await _keycloak.DeleteUserAsync(user.ExternalId);

                // Delete user in our database
                user.MarkDeleted();
                await _userRepository.DeleteAsync(user, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId} (externalId: {ExternalId})", user.Id, user.ExternalId);
                throw;
            }
        }
    }
}
