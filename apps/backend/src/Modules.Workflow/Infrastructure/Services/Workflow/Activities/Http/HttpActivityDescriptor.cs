using System.ComponentModel.DataAnnotations;
using Modules.Workflow.Domain.Activities;

namespace Modules.Workflow.Infrastructure.Services.Workflow.Activities.Http
{
    public class HttpActivityDescriptor : ActivityDescriptorBase
    {
        private static readonly List<PropertyDescriptor> _inputProperties = new()
        {
            new PropertyDescriptor
            {
                Name = "url",
                DisplayName = "URL",
                Description = "The HTTP endpoint URL",
                DataType = "string",
                IsRequired = true,
                Validators = new List<ValidationAttribute>
                {
                    new RequiredAttribute(),
                    new UrlAttribute()
                }
            },
            new PropertyDescriptor
            {
                Name = "method",
                DisplayName = "Method",
                Description = "HTTP method (GET, POST, PUT, DELETE, etc.)",
                DataType = "string",
                IsRequired = true,
                DefaultValue = "GET"
            },
            new PropertyDescriptor
            {
                Name = "headers",
                DisplayName = "Headers",
                Description = "HTTP headers (JSON object)",
                DataType = "object",
                IsRequired = false
            },
            new PropertyDescriptor
            {
                Name = "body",
                DisplayName = "Body",
                Description = "HTTP body (JSON object)",
                DataType = "object",
                IsRequired = false
            },
            new PropertyDescriptor
            {
                Name = "query",
                DisplayName = "Query Parameters",
                Description = "Query parameters (JSON object)",
                DataType = "object",
                IsRequired = false
            },
        };

        private static readonly List<PropertyDescriptor> _outputProperties = new()
        {
            new PropertyDescriptor
            {
                Name = "statusCode",
                DisplayName = "Status Code",
                Description = "HTTP response status code",
                DataType = "int",
            },
            new PropertyDescriptor
            {
                Name = "responseBody",
                DisplayName = "Response Body",
                Description = "HTTP response body",
                DataType = "string",
            },
            new PropertyDescriptor
            {
                Name = "responseHeaders",
                DisplayName = "Response Headers",
                Description = "HTTP response headers",
                DataType = "object",
            },
            new PropertyDescriptor
            {
                Name = "error",
                DisplayName = "Error",
                Description = "Error message if request fails",
                DataType = "string",
            }
        };
        
        public override string ActivityType => "ZenFlow.Activities.Http.HttpActivity";
        public override string DisplayName => "HTTP Request";
        public override string Description => "Sends an HTTP request and returns the response";
        public override string Category => "HTTP";
        public override IEnumerable<PropertyDescriptor> InputProperties => _inputProperties;
        public override IEnumerable<PropertyDescriptor> OutputProperties => _outputProperties;
    }
}