using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Modules.Workflow.Infrastructure.Outbox;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Workflow.Infrastructure.Events
{
    public class WorkflowOutboxCleaner : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<WorkflowOutboxCleaner> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromHours(1);
        private readonly TimeSpan _retentionPeriod = TimeSpan.FromDays(7);

        public WorkflowOutboxCleaner(
            IServiceScopeFactory scopeFactory,
            ILogger<WorkflowOutboxCleaner> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Workflow Outbox Cleaner started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanOutboxMessagesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error cleaning workflow outbox messages");
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }

        private async Task CleanOutboxMessagesAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var outboxRepository = scope.ServiceProvider.GetRequiredService<IWorkflowOutboxRepository>();

            var cutoffDate = DateTime.UtcNow.Subtract(_retentionPeriod);
            await outboxRepository.DeleteProcessedMessagesOlderThanAsync(cutoffDate);

            _logger.LogInformation("Cleaned processed workflow outbox messages older than {CutoffDate}", cutoffDate);
        }
    }
}