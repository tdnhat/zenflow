using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace Modules.Workflow.Features.WorkflowExecutions.RunWorkflow.ActivityMappers
{
    /// <summary>
    /// Default implementation of the activity mapper factory
    /// </summary>
    public class ActivityMapperFactory : IActivityMapperFactory
    {
        private readonly IEnumerable<IActivityMapper> _mappers;
        private readonly ILogger<ActivityMapperFactory> _logger;

        public ActivityMapperFactory(
            IEnumerable<IActivityMapper> mappers,
            ILogger<ActivityMapperFactory> logger)
        {
            _mappers = mappers;
            _logger = logger;
        }

        /// <summary>
        /// Gets the appropriate mapper for a given activity type
        /// </summary>
        public IActivityMapper? GetMapper(string activityType)
        {
            // Find the first mapper that can handle this activity type
            var mapper = _mappers.FirstOrDefault(m => m.CanMap(activityType));
            
            if (mapper == null)
            {
                _logger.LogWarning("No mapper found for activity type: {ActivityType}", activityType);
            }
            
            return mapper;
        }
    }
} 