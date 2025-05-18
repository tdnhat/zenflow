using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Modules.Workflow.Domain.Interfaces.Core;

namespace Modules.Workflow.Domain.Activities
{
    /// <summary>
    /// Catalog of all registered activity types and their descriptors
    /// </summary>
    public static class ActivityCatalog
    {
        private static readonly ConcurrentDictionary<string, IActivityDescriptor> _activityDescriptors = new();
        
        /// <summary>
        /// Register an activity descriptor for discovery and UI
        /// </summary>
        public static void RegisterActivityDescriptor(IActivityDescriptor descriptor)
        {
            if (descriptor == null)
                throw new ArgumentNullException(nameof(descriptor));
                
            if (string.IsNullOrEmpty(descriptor.ActivityType))
                throw new ArgumentException("Activity type cannot be null or empty", nameof(descriptor));
                
            _activityDescriptors[descriptor.ActivityType] = descriptor;
        }
        
        /// <summary>
        /// Get all registered activity descriptors
        /// </summary>
        public static IEnumerable<IActivityDescriptor> GetAllActivityDescriptors()
        {
            return _activityDescriptors.Values;
        }
        
        /// <summary>
        /// Get a specific activity descriptor by type
        /// </summary>
        public static IActivityDescriptor GetActivityDescriptor(string activityType)
        {
            if (string.IsNullOrEmpty(activityType))
                throw new ArgumentException("Activity type cannot be null or empty", nameof(activityType));
                
            if (_activityDescriptors.TryGetValue(activityType, out var descriptor))
                return descriptor;
                
            throw new ArgumentException($"No activity descriptor found for type '{activityType}'");
        }
        
        /// <summary>
        /// Check if an activity type is registered
        /// </summary>
        public static bool IsActivityTypeRegistered(string activityType)
        {
            if (string.IsNullOrEmpty(activityType))
                return false;
                
            return _activityDescriptors.ContainsKey(activityType);
        }
        
        /// <summary>
        /// Get activity descriptors by category
        /// </summary>
        public static IEnumerable<IActivityDescriptor> GetActivityDescriptorsByCategory(string category)
        {
            if (string.IsNullOrEmpty(category))
                throw new ArgumentException("Category cannot be null or empty", nameof(category));
                
            return _activityDescriptors.Values.Where(d => d.Category == category);
        }
    }
    
    /// <summary>
    /// Helper for registering activities and their descriptors
    /// </summary>
    public static class ActivityRegistration
    {
        /// <summary>
        /// Register an activity with its executor and descriptor
        /// </summary>
        public static IServiceCollection RegisterActivity<TExecutor>(
            this IServiceCollection services,
            IActivityDescriptor descriptor) 
            where TExecutor : class, IActivityExecutor
        {
            // Register the executor
            services.AddScoped<TExecutor>();
            services.AddScoped<IActivityExecutor, TExecutor>();
            
            // Register the descriptor
            ActivityCatalog.RegisterActivityDescriptor(descriptor);
            
            return services;
        }
    }
} 