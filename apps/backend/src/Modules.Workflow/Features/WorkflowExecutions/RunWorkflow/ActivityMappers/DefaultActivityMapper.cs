using Elsa.Workflows;
using Elsa.Workflows.Activities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Modules.Workflow.Infrastructure.Services.BrowserAutomation.Activities;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;

namespace Modules.Workflow.Features.WorkflowExecutions.RunWorkflow.ActivityMappers
{
    /// <summary>
    /// Default mapper that attempts to create any activity type using reflection
    /// </summary>
    public class DefaultActivityMapper : IActivityMapper
    {
        private readonly ILogger<DefaultActivityMapper> _logger;
        private static readonly Dictionary<string, Type> _activityTypeCache = new();

        public DefaultActivityMapper(ILogger<DefaultActivityMapper> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// This mapper is a fallback for any activity type not handled by other mappers
        /// </summary>
        public bool CanMap(string activityType)
        {
            return true;
        }

        /// <summary>
        /// Maps an activity configuration to an activity instance using reflection
        /// </summary>
        public IActivity MapToActivity(string activityType, Dictionary<string, object> config, IServiceProvider serviceProvider)
        {
            _logger.LogDebug("Attempting to map activity type: {ActivityType} with default mapper", activityType);
            
            try
            {
                // Try to find the activity type with or without the Activity suffix
                Type? activityClassType = FindActivityType(activityType);
                
                if (activityClassType == null)
                {
                    _logger.LogError("Could not find activity type for: {ActivityType}", activityType);
                    throw new InvalidOperationException($"Could not find activity type for: {activityType}");
                }
                
                // Create instance of the activity
                IActivity activity = (IActivity)ActivatorUtilities.CreateInstance(serviceProvider, activityClassType);
                
                // Set properties based on config
                ApplyConfiguration(activity, config);
                
                return activity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create activity for type {ActivityType}: {ErrorMessage}", activityType, ex.Message);
                throw;
            }
        }
        
        /// <summary>
        /// Attempts to find the activity type by name, with or without the Activity suffix
        /// </summary>
        private Type? FindActivityType(string activityType)
        {
            // Check if we've already found this type
            if (_activityTypeCache.TryGetValue(activityType, out var cachedType))
            {
                return cachedType;
            }
            
            // Normalize activity type name (to PascalCase and ensure it ends with "Activity")
            string normalizedName = activityType.Substring(0, 1).ToUpperInvariant() + activityType.Substring(1);
            if (!normalizedName.EndsWith("Activity", StringComparison.OrdinalIgnoreCase))
            {
                normalizedName += "Activity";
            }
            
            // Get all types from the Elsa.Workflows.Activities and our own activities namespace
            var assemblies = new[] 
            { 
                typeof(Sequence).Assembly, // Elsa core activities
                typeof(ManualTriggerActivity).Assembly // Our activities
            };
            
            foreach (var assembly in assemblies)
            {
                // First try with exact match
                var type = assembly.GetType($"Elsa.Workflows.Activities.{normalizedName}") ??
                           assembly.GetType($"Modules.Workflow.Services.BrowserAutomation.Activities.{normalizedName}");
                
                if (type != null)
                {
                    _activityTypeCache[activityType] = type;
                    return type;
                }
                
                // Try finding by just the name part
                foreach (var candidateType in assembly.GetTypes())
                {
                    if (candidateType.Name.Equals(normalizedName, StringComparison.OrdinalIgnoreCase) &&
                        typeof(IActivity).IsAssignableFrom(candidateType))
                    {
                        _activityTypeCache[activityType] = candidateType;
                        return candidateType;
                    }
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// Applies configuration to the activity instance
        /// </summary>
        private void ApplyConfiguration(IActivity activity, Dictionary<string, object> config)
        {
            var activityType = activity.GetType();
            
            foreach (var kvp in config)
            {
                try
                {
                    // Find property with this name (case-insensitive)
                    var property = activityType.GetProperty(kvp.Key, 
                        BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    
                    if (property != null && property.CanWrite)
                    {
                        // Convert value to the property type if needed
                        object? value = kvp.Value;
                        
                        if (value != null && property.PropertyType != value.GetType())
                        {
                            if (value is JsonElement jsonElement)
                            {
                                value = ConvertJsonElement(jsonElement, property.PropertyType);
                            }
                            else
                            {
                                value = Convert.ChangeType(value, property.PropertyType);
                            }
                        }
                        
                        property.SetValue(activity, value);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to set property {PropertyName} on activity {ActivityType}: {ErrorMessage}", 
                        kvp.Key, activityType.Name, ex.Message);
                    // Continue with other properties
                }
            }
        }
        
        /// <summary>
        /// Converts a JsonElement to the specified type
        /// </summary>
        private object? ConvertJsonElement(JsonElement element, Type targetType)
        {
            switch (element.ValueKind)
            {
                case System.Text.Json.JsonValueKind.String:
                    return Convert.ChangeType(element.GetString(), targetType);
                    
                case System.Text.Json.JsonValueKind.Number:
                    if (targetType == typeof(int) || targetType == typeof(int?))
                        return element.GetInt32();
                    if (targetType == typeof(double) || targetType == typeof(double?))
                        return element.GetDouble();
                    if (targetType == typeof(decimal) || targetType == typeof(decimal?))
                        return element.GetDecimal();
                    if (targetType == typeof(float) || targetType == typeof(float?))
                        return element.GetSingle();
                    if (targetType == typeof(long) || targetType == typeof(long?))
                        return element.GetInt64();
                    return Convert.ChangeType(element.GetDouble(), targetType);
                    
                case System.Text.Json.JsonValueKind.True:
                    return true;
                    
                case System.Text.Json.JsonValueKind.False:
                    return false;
                    
                case System.Text.Json.JsonValueKind.Null:
                    return null;
                    
                default:
                    return element.GetRawText();
            }
        }
    }
} 