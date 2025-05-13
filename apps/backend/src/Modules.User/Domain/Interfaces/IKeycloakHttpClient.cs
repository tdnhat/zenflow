using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Modules.User.Domain.Interfaces
{
    /// Client for making HTTP requests to the Keycloak API
    public interface IKeycloakHttpClient
    {
        Task SendApiRequestAsync<T>(HttpMethod method, string url, T data);

        Task<HttpResponseMessage> SendApiRequestWithResponseAsync(HttpMethod method, string url, object? data = null);

        JsonSerializerOptions JsonOptions { get; }
    }
}