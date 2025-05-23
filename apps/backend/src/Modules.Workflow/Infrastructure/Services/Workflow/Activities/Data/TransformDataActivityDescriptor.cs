using System.ComponentModel.DataAnnotations;
using Modules.Workflow.Domain.Activities;

namespace Modules.Workflow.Infrastructure.Services.Workflow.Activities.Data
{
    public class TransformDataActivityDescriptor : ActivityDescriptorBase
    {
        private static readonly List<PropertyDescriptor> _inputProperties = new()
        {
            new PropertyDescriptor
            {
                Name = "transformationType",
                DisplayName = "Transformation Type",
                Description = "The type of transformation to perform (json, regex, text, format)",
                DataType = "string",
                IsRequired = true,
                Validators = new List<ValidationAttribute>
                {
                    new RequiredAttribute()
                }
            },
            new PropertyDescriptor
            {
                Name = "inputProperty",
                DisplayName = "Input Property",
                Description = "Name of the input property containing the data to transform",
                DataType = "string",
                IsRequired = true,
                Validators = new List<ValidationAttribute>
                {
                    new RequiredAttribute()
                }
            },
            new PropertyDescriptor
            {
                Name = "outputProperty",
                DisplayName = "Output Property",
                Description = "Name of the output property to store the transformed data",
                DataType = "string",
                IsRequired = true,
                Validators = new List<ValidationAttribute>
                {
                    new RequiredAttribute()
                }
            },
            new PropertyDescriptor
            {
                Name = "jsonPath",
                DisplayName = "JSON Path",
                Description = "JSONPath expression for extracting data from JSON (required for 'json' transformation type)",
                DataType = "string",
                IsRequired = false
            },
            new PropertyDescriptor
            {
                Name = "regexPattern",
                DisplayName = "Regex Pattern",
                Description = "Regular expression pattern for finding text to replace (required for 'regex' transformation type)",
                DataType = "string",
                IsRequired = false
            },
            new PropertyDescriptor
            {
                Name = "replaceValue",
                DisplayName = "Replace Value",
                Description = "Replacement value for regex pattern matches (required for 'regex' transformation type)",
                DataType = "string",
                IsRequired = false
            },
            new PropertyDescriptor
            {
                Name = "transformationExpression",
                DisplayName = "Transformation Expression",
                Description = "Expression or template for text transformation or string formatting",
                DataType = "string",
                IsRequired = false
            }
        };

        private static readonly List<PropertyDescriptor> _outputProperties = new()
        {
            new PropertyDescriptor
            {
                Name = "transformedData",
                DisplayName = "Transformed Data",
                Description = "The result of the data transformation",
                DataType = "object",
                IsRequired = false
            },
            new PropertyDescriptor
            {
                Name = "success",
                DisplayName = "Success",
                Description = "Whether the transformation was successful",
                DataType = "boolean",
                IsRequired = false
            },
            new PropertyDescriptor
            {
                Name = "error",
                DisplayName = "Error",
                Description = "Error information if transformation failed",
                DataType = "object",
                IsRequired = false
            }
        };

        public override string ActivityType => "ZenFlow.Activities.Data.TransformDataActivity";
        public override string DisplayName => "Transform Data";
        public override string Description => "Transform and manipulate data using various transformation methods including JSON path extraction, regex replace, text transformation, and string formatting";
        public override string Category => "Data Operations";
        public override IEnumerable<PropertyDescriptor> InputProperties => _inputProperties;
        public override IEnumerable<PropertyDescriptor> OutputProperties => _outputProperties;
    }
} 