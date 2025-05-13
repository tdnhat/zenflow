using MediatR;
using Microsoft.Extensions.Logging;
using Modules.User.Domain.Interfaces;
using Modules.User.Infrastructure.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.User.Features.DeleteUserById
{
    public class DeleteUserByIdHandler : IRequestHandler<DeleteUserByIdCommand>
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
            var user = request.Permanent ?
                await _userRepository.GetByIdIncludingDeletedAsync(request.Id, cancellationToken) :
                await _userRepository.GetByIdAsync(request.Id, cancellationToken);

            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found", request.Id);
                throw new NotFoundException($"User with ID {request.Id} not found");
            }

            _logger.LogInformation("Deleting user {UserId} (externalId: {ExternalId}), Permanent: {Permanent}",
                user.Id, user.ExternalId, request.Permanent);

            try
            {
                if (request.Permanent)
                {
                    // Delete user from Keycloak
                    await _keycloak.DeleteUserAsync(user.ExternalId);

                    // Hard delete from app DB
                    user.MarkDeleted();
                    await _userRepository.PermanentlyDeleteAsync(user, cancellationToken);
                    _logger.LogInformation("User {UserId} permanently deleted", user.Id);
                }
                else
                {
                    // Soft delete in app DB
                    await _userRepository.DeleteAsync(user, cancellationToken);

                    // Deactivate user in Keycloak
                    await _keycloak.SetUserActiveStatusAsync(user.ExternalId, false);
                    _logger.LogInformation("User {UserId} soft-deleted and deactivated in Keycloak", user.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId} (externalId: {ExternalId})", user.Id, user.ExternalId);
                throw;
            }
        }
    }
}
