using Elsa.Extensions;
using Elsa.Workflows;
using Microsoft.Extensions.Logging;
using Modules.Workflow.DDD.Interfaces;

namespace Modules.Workflow.Services.BrowserAutomation.Activities
{
    public class InputTextActivity : CodeActivity
    {
        private readonly IBrowserSessionManager _browserSessionManager;
        private readonly ILogger<InputTextActivity> _logger;

        public InputTextActivity(
            IBrowserSessionManager browserSessionManager,
            ILogger<InputTextActivity> logger)
        {
            _browserSessionManager = browserSessionManager;
            _logger = logger;
        }

        public string Selector { get; set; } = default!;
        public string Text { get; set; } = default!;
        public bool ClearFirst { get; set; } = true;
        public int TypeDelay { get; set; } = 0;

        protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
        {
            var workflowInstanceId = context.WorkflowExecutionContext.Id.ToString();
            _logger.LogInformation("Executing InputTextActivity for workflow {WorkflowInstanceId} with selector {Selector}", 
                workflowInstanceId, Selector);
                
            var page = await _browserSessionManager.GetOrCreatePageForWorkflowAsync(workflowInstanceId, context.CancellationToken);
            var selector = Selector;
            var text = Text;

            try
            {
                // Find the element
                _logger.LogDebug("Finding input element with selector {Selector}", selector);
                var element = await page.QuerySelectorAsync(selector);
                if (element == null)
                {
                    _logger.LogError("Input element not found with selector: {Selector}", selector);
                    throw new Exception($"Element not found with selector: {selector}");
                }

                // Clear the field first if required
                if (ClearFirst)
                {
                    _logger.LogDebug("Clearing field before input");
                    await element.FillAsync("");
                }

                // Type the text
                if (TypeDelay > 0)
                {
                    // Human-like typing with delay
                    _logger.LogDebug("Using human-like typing with {TypeDelay}ms delay between characters", TypeDelay);
                    await element.FocusAsync();
                    foreach (var character in text)
                    {
                        await page.Keyboard.TypeAsync(character.ToString());
                        await Task.Delay(TypeDelay, context.CancellationToken);
                    }
                }
                else
                {
                    // Fast fill
                    _logger.LogDebug("Fast filling text into element (length: {TextLength})", text.Length);
                    await element.FillAsync(text);
                }
                
                _logger.LogInformation("Successfully input text into element with selector {Selector}", selector);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to input text into element with selector {Selector}: {ErrorMessage}", selector, ex.Message);
                throw new Exception($"Input operation failed: {ex.Message}", ex);
            }
        }
    }
} 