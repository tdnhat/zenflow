using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Modules.User.DDD.Interfaces;
using Modules.User.Infrastructure.Outbox;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.User.Infrastructure.Events
{
    public class UserOutboxCleaner : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<UserOutboxCleaner> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromHours(1);
        private readonly TimeSpan _retentionPeriod = TimeSpan.FromDays(7);

        public UserOutboxCleaner(
            IServiceScopeFactory scopeFactory,
            ILogger<UserOutboxCleaner> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("User Outbox Cleaner started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanOutboxMessagesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error cleaning user outbox messages");
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }

        private async Task CleanOutboxMessagesAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var outboxRepository = scope.ServiceProvider.GetRequiredService<IUserOutboxRepository>();

            var cutoffDate = DateTime.UtcNow.Subtract(_retentionPeriod);
            await outboxRepository.DeleteProcessedMessagesOlderThanAsync(cutoffDate);

            _logger.LogInformation("Cleaned processed user outbox messages older than {CutoffDate}", cutoffDate);
        }
    }
}