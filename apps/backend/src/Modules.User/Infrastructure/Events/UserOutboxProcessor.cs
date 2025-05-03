using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Modules.User.Infrastructure.Outbox;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ZenFlow.Shared.Domain;
using MassTransit;
using Modules.User.DDD.Interfaces;

namespace Modules.User.Infrastructure.Events
{
    public class UserOutboxProcessor : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<UserOutboxProcessor> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromSeconds(10);
        private readonly int _batchSize = 20;

        public UserOutboxProcessor(
            IServiceScopeFactory scopeFactory,
            ILogger<UserOutboxProcessor> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("User Outbox Processor started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessOutboxMessagesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing user outbox messages");
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }

        private async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var outboxRepository = scope.ServiceProvider.GetRequiredService<IUserOutboxRepository>();
            var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

            var messages = await outboxRepository.GetUnprocessedMessagesAsync(_batchSize);
            if (messages.Count == 0)
            {
                return;
            }

            _logger.LogInformation("Processing {Count} user outbox messages", messages.Count);

            foreach (var message in messages)
            {
                try
                {
                    if (string.IsNullOrEmpty(message.EventType))
                    {
                        message.Error = "Event type is null or empty";
                        message.ProcessedOn = DateTime.UtcNow;
                        await outboxRepository.UpdateAsync(message);
                        continue;
                    }

                    var eventType = Type.GetType(message.EventType);
                    if (eventType == null)
                    {
                        message.Error = $"Event type '{message.EventType}' not found";
                        message.ProcessedOn = DateTime.UtcNow;
                        await outboxRepository.UpdateAsync(message);
                        continue;
                    }

                    var domainEvent = JsonSerializer.Deserialize(message.EventContent, eventType) as IDomainEvent;
                    if (domainEvent == null)
                    {
                        message.Error = "Failed to deserialize event";
                        message.ProcessedOn = DateTime.UtcNow;
                        await outboxRepository.UpdateAsync(message);
                        continue;
                    }

                    await publishEndpoint.Publish(domainEvent, eventType, cancellationToken);

                    message.ProcessedOn = DateTime.UtcNow;
                    await outboxRepository.UpdateAsync(message);

                    _logger.LogInformation("Published user event {EventType} with ID {Id}", 
                        message.EventType, message.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing user outbox message {Id}", message.Id);
                    message.Error = ex.Message;
                    message.RetryCount++;
                    await outboxRepository.UpdateAsync(message);
                }
            }
        }
    }
}