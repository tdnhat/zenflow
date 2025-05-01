using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace ZenFlow.Shared.Behaviors
{
    public class LoggingBehavior<TRequest, TResponse>
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

        public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;
            var requestId = Guid.NewGuid().ToString();

            _logger.LogInformation("[START] {RequestName} {RequestId}", requestName, requestId);

            var stopwatch = Stopwatch.StartNew();
            try
            {
                var response = await next();
                stopwatch.Stop();

                _logger.LogInformation("[END] {RequestName} {RequestId}; Execution time={ElapsedMilliseconds}ms",
                    requestName, requestId, stopwatch.ElapsedMilliseconds);

                return response;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "[ERROR] {RequestName} {RequestId}; Execution time={ElapsedMilliseconds}ms",
                    requestName, requestId, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }
    }
}