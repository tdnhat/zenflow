using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Modules.Workflow.Activities;
using Modules.Workflow.Domain.Core;
using Modules.Workflow.Domain.Enums;

namespace Modules.Workflow.Infrastructure.Services.Workflow.Activities.Http
{
    public class HttpActivityExecutor : BaseActivityExecutor
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public HttpActivityExecutor(
            ILogger<HttpActivityExecutor> logger,
            IHttpClientFactory httpClientFactory)
            : base(logger)
        {
            _httpClientFactory = httpClientFactory;
        }

        public override bool CanExecute(string activityType)
        {
            return activityType == "ZenFlow.Activities.Http.HttpRequestActivity";
        }

        public override async Task<(ActivityExecutionResult, Dictionary<string, object>)> ExecuteAsync(
            string activityType,
            Dictionary<string, object> activityProperties,
            Dictionary<string, object> inputData,
            WorkflowExecutionContext workflowContext,
            NodeExecutionContext nodeExecutionContext,
            CancellationToken cancellationToken = default
        )
        {
            return await ExecuteWithErrorHandlingAsync(async () =>
            {
                nodeExecutionContext.AddLog($"Executing HTTP activity: {activityType}");

                var url = GetPropertyValue<string>(activityProperties, inputData, "url");
                var method = GetPropertyValue<string>(activityProperties, inputData, "method")?.ToUpperInvariant() ?? "GET";
                var headers = GetPropertyValueOrDefault<Dictionary<string, string>>(activityProperties, inputData, "headers", null);
                var body = GetPropertyValueOrDefault<string>(activityProperties, inputData, "body", null);
                var query = GetPropertyValueOrDefault<Dictionary<string, string>>(activityProperties, inputData, "query", null);

                // Build query string if query parameters exist
                if (query != null && query.Any())
                {
                    var queryString = string.Join("&", query.Select(
                        kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value?.ToString() ?? "")}"
                    ));

                    url += url.Contains("?") ? "&" : "?";
                    url += queryString;
                }

                var httpClient = _httpClientFactory.CreateClient("WorkflowHttpClient");
                using var request = new HttpRequestMessage(new HttpMethod(method), url);

                // Add headers
                if (headers != null && headers.Any())
                {
                    foreach (var header in headers)
                    {
                        request.Headers.TryAddWithoutValidation(header.Key, header.Value?.ToString() ?? "");
                    }
                }

                // Add body for POST/PUT/PATCH
                if (!string.IsNullOrEmpty(body) && (method == "POST" || method == "PUT" || method == "PATCH"))
                {
                    request.Content = new StringContent(body, Encoding.UTF8, MediaTypeHeaderValue.Parse("application/json"));
                }

                // Send request
                HttpResponseMessage response;
                try
                {
                    response = await httpClient.SendAsync(request, cancellationToken);
                }
                catch (Exception ex)
                {
                    nodeExecutionContext.AddLog($"Error sending HTTP request: {ex.Message}");
                    return (ActivityExecutionResult.Failed, new Dictionary<string, object>
                    {
                        ["error"] = new { message = ex.Message }
                    });
                }

                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                var responseHeaders = response.Headers.ToDictionary(h => h.Key, h => (object)string.Join(", ", h.Value));

                nodeExecutionContext.AddLog($"HTTP Response status: {response.StatusCode}");

                return (ActivityExecutionResult.Completed, new Dictionary<string, object>
                {
                    ["statusCode"] = (int)response.StatusCode,
                    ["responseBody"] = responseBody,
                    ["responseHeaders"] = responseHeaders
                });
            }, nodeExecutionContext);
        }

        private T GetPropertyValue<T>(
    Dictionary<string, object> activityProperties,
    Dictionary<string, object> inputData,
    string propertyName)
        {
            // Case-insensitive property lookup

            // Try to find the property in input data first
            var inputKey = inputData.Keys
                .FirstOrDefault(k => string.Equals(k, propertyName, StringComparison.OrdinalIgnoreCase));

            if (inputKey != null && inputData.TryGetValue(inputKey, out var inputValue))
            {
                if (inputValue is T typedValue)
                {
                    return typedValue;
                }

                // Handle JsonElement conversion
                if (inputValue is JsonElement jsonElement)
                {
                    return ConvertJsonElement<T>(jsonElement);
                }

                // Try to convert the value
                try
                {
                    return (T)Convert.ChangeType(inputValue, typeof(T));
                }
                catch
                {
                    // Ignore conversion errors and continue checking properties
                }
            }

            // Then check activity properties
            var propKey = activityProperties.Keys
                .FirstOrDefault(k => string.Equals(k, propertyName, StringComparison.OrdinalIgnoreCase));

            if (propKey != null && activityProperties.TryGetValue(propKey, out var propValue))
            {
                if (propValue is T propTypedValue)
                {
                    return propTypedValue;
                }

                // Handle JsonElement conversion
                if (propValue is JsonElement jsonElement)
                {
                    return ConvertJsonElement<T>(jsonElement);
                }

                // Try to convert the value
                try
                {
                    return (T)Convert.ChangeType(propValue, typeof(T));
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Property '{propertyName}' found but could not be converted to type {typeof(T).Name}. Error: {ex.Message}");
                }
            }

            // If not found, throw an error
            throw new InvalidOperationException($"Property '{propertyName}' not found in input data or activity properties");
        }

        // Add this helper method for JsonElement conversion
        private T ConvertJsonElement<T>(JsonElement element)
        {
            Type targetType = typeof(T);

            if (targetType == typeof(string))
            {
                return (T)(object)element.GetString();
            }
            else if (targetType == typeof(int))
            {
                return (T)(object)element.GetInt32();
            }
            else if (targetType == typeof(long))
            {
                return (T)(object)element.GetInt64();
            }
            else if (targetType == typeof(double))
            {
                return (T)(object)element.GetDouble();
            }
            else if (targetType == typeof(bool))
            {
                return (T)(object)element.GetBoolean();
            }
            else if (targetType == typeof(DateTime))
            {
                return (T)(object)element.GetDateTime();
            }
            else
            {
                // For more complex types, use JsonSerializer
                return JsonSerializer.Deserialize<T>(element.GetRawText());
            }
        }

        // Also add this convenience method for optional properties
        private T GetPropertyValueOrDefault<T>(
            Dictionary<string, object> activityProperties,
            Dictionary<string, object> inputData,
            string propertyName,
            T defaultValue)
        {
            try
            {
                return GetPropertyValue<T>(activityProperties, inputData, propertyName);
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}