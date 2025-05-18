using System.ComponentModel.DataAnnotations;
using Modules.Workflow.Domain.Activities;

namespace Modules.Workflow.Infrastructure.Services.Workflow.Activities.Email
{
    /// <summary>
    /// Descriptor for the email send activity
    /// </summary>
    public class SendEmailActivityDescriptor : ActivityDescriptorBase
    {
        private static readonly List<PropertyDescriptor> _inputProperties = new()
        {
            new PropertyDescriptor
            {
                Name = "to",
                DisplayName = "To",
                Description = "Recipient email address",
                DataType = "string",
                IsRequired = true,
                Validators = new List<ValidationAttribute>
                {
                    new RequiredAttribute(),
                    new EmailAddressAttribute()
                }
            },
            new PropertyDescriptor
            {
                Name = "subject",
                DisplayName = "Subject",
                Description = "Email subject line",
                DataType = "string",
                IsRequired = true,
                Validators = new List<ValidationAttribute>
                {
                    new RequiredAttribute(),
                    new StringLengthAttribute(255) { MinimumLength = 1 }
                }
            },
            new PropertyDescriptor
            {
                Name = "body",
                DisplayName = "Body",
                Description = "Email body content",
                DataType = "string",
                IsRequired = true,
                Validators = new List<ValidationAttribute>
                {
                    new RequiredAttribute()
                }
            },
            new PropertyDescriptor
            {
                Name = "isHtml",
                DisplayName = "Is HTML",
                Description = "Whether the body contains HTML content",
                DataType = "boolean",
                IsRequired = false,
                DefaultValue = false
            },
            new PropertyDescriptor
            {
                Name = "cc",
                DisplayName = "CC",
                Description = "Carbon copy recipients (comma-separated)",
                DataType = "string",
                IsRequired = false
            },
            new PropertyDescriptor
            {
                Name = "bcc",
                DisplayName = "BCC",
                Description = "Blind carbon copy recipients (comma-separated)",
                DataType = "string",
                IsRequired = false
            }
        };

        private static readonly List<PropertyDescriptor> _outputProperties = new()
        {
            new PropertyDescriptor
            {
                Name = "success",
                DisplayName = "Success",
                Description = "Whether the email was sent successfully",
                DataType = "boolean"
            },
            new PropertyDescriptor
            {
                Name = "error",
                DisplayName = "Error",
                Description = "Error details if the email failed to send",
                DataType = "object"
            }
        };

        public override string ActivityType => "ZenFlow.Activities.Email.SendEmailActivity";
        
        public override string DisplayName => "Send Email";
        
        public override string Description => "Sends an email to specified recipients";
        
        public override string Category => "Email";
        
        public override IEnumerable<PropertyDescriptor> InputProperties => _inputProperties;
        
        public override IEnumerable<PropertyDescriptor> OutputProperties => _outputProperties;
    }
} 