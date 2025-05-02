using Elsa.Extensions;
using Elsa.Workflows;
using Microsoft.Extensions.Logging;
using Modules.Workflow.DDD.Interfaces;

namespace Modules.Workflow.Services.BrowserAutomation.Activities
{
    public class ExtractDataActivity : CodeActivity
    {
        private readonly IBrowserSessionManager _browserSessionManager;
        private readonly ILogger<ExtractDataActivity> _logger;

        public ExtractDataActivity(
            IBrowserSessionManager browserSessionManager,
            ILogger<ExtractDataActivity> logger)
        {
            _browserSessionManager = browserSessionManager;
            _logger = logger;
        }

        public string Selector { get; set; } = default!;
        public string PropertyToExtract { get; set; } = "innerText";
        public string OutputVariableName { get; set; } = "extractedData";
        public bool ExtractAll { get; set; } = false;

        protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
        {
            var workflowInstanceId = context.WorkflowExecutionContext.Id.ToString();
            _logger.LogInformation("Executing ExtractDataActivity for workflow {WorkflowInstanceId} with selector {Selector}", 
                workflowInstanceId, Selector);
                
            var page = await _browserSessionManager.GetOrCreatePageForWorkflowAsync(workflowInstanceId, context.CancellationToken);
            var selector = Selector;
            object extractedData;

            try
            {
                if (ExtractAll)
                {
                    // Extract from all matching elements
                    _logger.LogDebug("Extracting property {PropertyToExtract} from all elements matching selector {Selector}", 
                        PropertyToExtract, selector);
                        
                    extractedData = await page.EvaluateAsync<string[]>($@"
                        Array.from(document.querySelectorAll('{selector}')).map(el => {{
                            return el.{PropertyToExtract} || '';
                        }})
                    ");
                    
                    if (extractedData is string[] dataArray)
                    {
                        _logger.LogDebug("Extracted {Count} items from elements", dataArray.Length);
                    }
                }
                else
                {
                    // Extract from first matching element
                    _logger.LogDebug("Extracting property {PropertyToExtract} from first element matching selector {Selector}", 
                        PropertyToExtract, selector);
                        
                    var element = await page.QuerySelectorAsync(selector);
                    if (element == null)
                    {
                        _logger.LogError("Element not found with selector: {Selector}", selector);
                        throw new Exception($"Element not found with selector: {selector}");
                    }

                    extractedData = await element.EvaluateAsync<string>($"el => el.{PropertyToExtract} || ''");
                    
                    _logger.LogDebug("Extracted value: {ExtractedData}", 
                        extractedData is string str && str.Length > 100 ? str.Substring(0, 100) + "..." : extractedData);
                }

                // Store extracted data in the specified variable
                _logger.LogInformation("Successfully extracted data into variable {VariableName}", OutputVariableName);
                context.SetVariable(OutputVariableName, extractedData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to extract data from element with selector {Selector}: {ErrorMessage}", 
                    selector, ex.Message);
                throw new Exception($"Data extraction failed: {ex.Message}", ex);
            }
        }
    }
} 