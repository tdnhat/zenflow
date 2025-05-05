using Elsa.Workflows;

namespace Modules.Workflow.Features.WorkflowExecutions.RunWorkflow.ActivityMappers
{
    /// <summary>
    /// Defines a contract for mapping workflow node configurations to Elsa activities
    /// </summary>
    public interface IActivityMapper
    {
        /// <summary>
        /// Checks if this mapper can handle the specified activity type
        /// </summary>
        /// <param name="activityType">The normalized activity type</param>
        /// <returns>True if this mapper can handle the activity type</returns>
        bool CanMap(string activityType);

        /// <summary>
        /// Maps a node configuration to an Elsa activity instance
        /// </summary>
        /// <param name="activityType">The normalized activity type</param>
        /// <param name="config">The configuration dictionary for the activity</param>
        /// <param name="serviceProvider">The service provider for dependency injection</param>
        /// <returns>A configured activity instance</returns>
        IActivity MapToActivity(string activityType, Dictionary<string, object> config, IServiceProvider serviceProvider);
    }
} 