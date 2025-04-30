using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Modules.User.DDD.Interfaces
{
    /// Client for making HTTP requests to the Keycloak API
    public interface IKeycloakHttpClient
    {
        /// Sends an API request to Keycloak with proper authentication
        Task SendApiRequestAsync<T>(HttpMethod method, string url, T data);
        
        /// Sends an API request to Keycloak with proper authentication and returns the response
        Task<HttpResponseMessage> SendApiRequestWithResponseAsync(HttpMethod method, string url, object? data = null);
        
        /// Gets the JSON serializer options used for Keycloak API requests
        JsonSerializerOptions JsonOptions { get; }
    }
}