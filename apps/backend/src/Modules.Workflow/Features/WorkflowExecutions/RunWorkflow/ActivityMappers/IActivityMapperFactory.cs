namespace Modules.Workflow.Features.WorkflowExecutions.RunWorkflow.ActivityMappers
{
    /// <summary>
    /// Factory for retrieving the appropriate activity mapper for a given activity type
    /// </summary>
    public interface IActivityMapperFactory
    {
        /// <summary>
        /// Gets the appropriate mapper for the specified activity type
        /// </summary>
        /// <param name="activityType">The normalized activity type</param>
        /// <returns>The activity mapper if found, null otherwise</returns>
        IActivityMapper? GetMapper(string activityType);
    }
} 