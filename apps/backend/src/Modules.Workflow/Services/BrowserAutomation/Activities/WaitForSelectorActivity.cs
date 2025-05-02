using Elsa.Extensions;
using Elsa.Workflows;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Modules.Workflow.DDD.Interfaces;

namespace Modules.Workflow.Services.BrowserAutomation.Activities
{
    public class WaitForSelectorActivity : CodeActivity
    {
        private readonly IBrowserSessionManager _browserSessionManager;
        private readonly ILogger<WaitForSelectorActivity> _logger;

        public WaitForSelectorActivity(
            IBrowserSessionManager browserSessionManager,
            ILogger<WaitForSelectorActivity> logger)
        {
            _browserSessionManager = browserSessionManager;
            _logger = logger;
        }

        public string Selector { get; set; } = default!;
        public int Timeout { get; set; } = 30000;
        public string State { get; set; } = "visible";

        protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
        {
            var workflowInstanceId = context.WorkflowExecutionContext.Id.ToString();
            _logger.LogInformation("Executing WaitForSelectorActivity for workflow {WorkflowInstanceId} with selector {Selector}", 
                workflowInstanceId, Selector);
            
            var page = await _browserSessionManager.GetOrCreatePageForWorkflowAsync(workflowInstanceId, context.CancellationToken);
            var selector = Selector;

            try
            {
                var state = State switch
                {
                    "hidden" => WaitForSelectorState.Hidden,
                    "attached" => WaitForSelectorState.Attached,
                    "detached" => WaitForSelectorState.Detached,
                    _ => WaitForSelectorState.Visible
                };

                _logger.LogDebug("Waiting for selector {Selector} with state {State} and timeout {Timeout}ms", 
                    selector, state, Timeout);
                
                await page.WaitForSelectorAsync(selector, new() { 
                    State = state,
                    Timeout = Timeout 
                });
                
                _logger.LogInformation("Successfully waited for selector {Selector}", selector);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to wait for selector {Selector}: {ErrorMessage}", selector, ex.Message);
                throw new Exception($"Wait for selector operation failed: {ex.Message}", ex);
            }
        }
    }
}

