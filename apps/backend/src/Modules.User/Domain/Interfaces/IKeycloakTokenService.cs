﻿namespace Modules.User.Domain.Interfaces
{
    /// <summary>
    /// Service for managing Keycloak authentication tokens
    /// </summary>
    public interface IKeycloakTokenService
    {
        /// Gets a valid admin token for Keycloak API operations
        Task<string> GetAdminTokenAsync();
        Task<string> ForceTokenRefreshAsync();
    }
}
