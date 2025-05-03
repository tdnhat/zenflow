using MassTransit;
using Microsoft.Extensions.Logging;
using Modules.Workflow.DDD.Events;
using System;
using System.Threading.Tasks;

namespace Modules.Workflow.Infrastructure.Consumers
{
    /// <summary>
    /// Sample consumer for the WorkflowCreatedEvent
    /// </summary>
    public class WorkflowCreatedConsumer : IConsumer<WorkflowCreatedEvent>
    {
        private readonly ILogger<WorkflowCreatedConsumer> _logger;

        public WorkflowCreatedConsumer(ILogger<WorkflowCreatedConsumer> logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<WorkflowCreatedEvent> context)
        {
            var @event = context.Message;
            
            _logger.LogInformation(
                "Consumed WorkflowCreatedEvent from RabbitMQ: Workflow {WorkflowId} with name '{Name}' was created at {OccurredOn}",
                @event.WorkflowId,
                @event.Name,
                @event.OccurredOn);
            
            // Here you would implement your business logic for handling this event
            // e.g., sending notifications, updating read models, triggering other processes, etc.
            
            return Task.CompletedTask;
        }
    }
}