using System.Net;

namespace Modules.User.Infrastructure.Exceptions
{
    public class KeycloakApiException : Exception
    {
        public HttpStatusCode? StatusCode { get; }
        public string? ResponseContent { get; }

        public KeycloakApiException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public KeycloakApiException(string message, HttpStatusCode statusCode, string responseContent)
            : base($"{message}. Status: {statusCode}, Response: {responseContent}")
        {
            StatusCode = statusCode;
            ResponseContent = responseContent;
        }
    }
}