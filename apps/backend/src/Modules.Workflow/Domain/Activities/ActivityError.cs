namespace Modules.Workflow.Domain.Activities
{
    /// <summary>
    /// Represents an error that occurred during activity execution
    /// </summary>
    public class ActivityError
    {
        /// <summary>
        /// Error code for categorizing the error
        /// </summary>
        public string Code { get; set; }
        
        /// <summary>
        /// User-friendly error message
        /// </summary>
        public string Message { get; set; }
        
        /// <summary>
        /// Additional details about the error
        /// </summary>
        public string Details { get; set; }
        
        /// <summary>
        /// The original exception, if any
        /// </summary>
        public Exception Exception { get; set; }
        
        /// <summary>
        /// When the error occurred
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Additional properties related to the error
        /// </summary>
        public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
        
        /// <summary>
        /// Create a new activity error
        /// </summary>
        public ActivityError() { }
        
        /// <summary>
        /// Create a new activity error with the specified code and message
        /// </summary>
        public ActivityError(string code, string message)
        {
            Code = code;
            Message = message;
        }
        
        /// <summary>
        /// Create a new activity error from an exception
        /// </summary>
        public ActivityError(Exception exception)
        {
            Code = "EXCEPTION";
            Message = exception.Message;
            Exception = exception;
            Details = exception.StackTrace;
        }
        
        /// <summary>
        /// Create a new validation error
        /// </summary>
        public static ActivityError ValidationError(string message, string propertyName = null)
        {
            var error = new ActivityError("VALIDATION_ERROR", message);
            
            if (!string.IsNullOrEmpty(propertyName))
            {
                error.Properties["PropertyName"] = propertyName;
            }
            
            return error;
        }
        
        /// <summary>
        /// Create a new not found error
        /// </summary>
        public static ActivityError NotFoundError(string entityType, string identifier)
        {
            return new ActivityError("NOT_FOUND", $"{entityType} with identifier '{identifier}' was not found")
            {
                Properties =
                {
                    ["EntityType"] = entityType,
                    ["Identifier"] = identifier
                }
            };
        }
        
        /// <summary>
        /// Create a new authorization error
        /// </summary>
        public static ActivityError AuthorizationError(string message)
        {
            return new ActivityError("AUTHORIZATION_ERROR", message);
        }
        
        /// <summary>
        /// Create a new configuration error
        /// </summary>
        public static ActivityError ConfigurationError(string message)
        {
            return new ActivityError("CONFIGURATION_ERROR", message);
        }
    }
    
    /// <summary>
    /// Extension methods for working with activity errors
    /// </summary>
    public static class ActivityErrorExtensions
    {
        /// <summary>
        /// Convert an exception to an activity error
        /// </summary>
        public static ActivityError ToActivityError(this Exception ex)
        {
            return new ActivityError(ex);
        }
    }
} 