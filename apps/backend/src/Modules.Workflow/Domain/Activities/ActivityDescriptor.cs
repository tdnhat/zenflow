using System.ComponentModel.DataAnnotations;

namespace Modules.Workflow.Domain.Activities
{
    /// <summary>
    /// Describes an activity that can be used in a workflow
    /// </summary>
    public interface IActivityDescriptor
    {
        /// <summary>
        /// The unique type identifier for this activity (e.g., "ZenFlow.Activities.Email.SendEmailActivity")
        /// </summary>
        string ActivityType { get; }
        
        /// <summary>
        /// Display name for the activity in the UI
        /// </summary>
        string DisplayName { get; }
        
        /// <summary>
        /// Description of what the activity does
        /// </summary>
        string Description { get; }
        
        /// <summary>
        /// Category for grouping related activities
        /// </summary>
        string Category { get; }
        
        /// <summary>
        /// Properties that this activity accepts as input
        /// </summary>
        IEnumerable<PropertyDescriptor> InputProperties { get; }
        
        /// <summary>
        /// Properties that this activity produces as output
        /// </summary>
        IEnumerable<PropertyDescriptor> OutputProperties { get; }
    }

    /// <summary>
    /// Describes a property of an activity
    /// </summary>
    public class PropertyDescriptor
    {
        /// <summary>
        /// The name of the property
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Display name for the property in the UI
        /// </summary>
        public string DisplayName { get; set; }
        
        /// <summary>
        /// Description of the property
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Data type of the property
        /// </summary>
        public string DataType { get; set; }
        
        /// <summary>
        /// Whether this property is required
        /// </summary>
        public bool IsRequired { get; set; }
        
        /// <summary>
        /// Default value for the property
        /// </summary>
        public object DefaultValue { get; set; }
        
        /// <summary>
        /// Validation attributes for the property
        /// </summary>
        public IEnumerable<ValidationAttribute> Validators { get; set; } = new List<ValidationAttribute>();
    }

    /// <summary>
    /// Base implementation of IActivityDescriptor
    /// </summary>
    public abstract class ActivityDescriptorBase : IActivityDescriptor
    {
        public abstract string ActivityType { get; }
        public abstract string DisplayName { get; }
        public abstract string Description { get; }
        public abstract string Category { get; }
        public abstract IEnumerable<PropertyDescriptor> InputProperties { get; }
        public abstract IEnumerable<PropertyDescriptor> OutputProperties { get; }
    }
} 