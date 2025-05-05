using Elsa.Extensions;
using Elsa.Workflows;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Modules.Workflow.DDD.Interfaces;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;

namespace Modules.Workflow.Services.BrowserAutomation.Activities
{
    /// <summary>
    /// An activity that extracts data from web elements using CSS selectors with enhanced reliability
    /// </summary>
    public class CrawlDataActivity : CodeActivity
    {
        private readonly IBrowserSessionManager _browserSessionManager;
        private readonly ILogger<CrawlDataActivity> _logger;

        /// <summary>
        /// Constructor for the CrawlDataActivity
        /// </summary>
        public CrawlDataActivity(
            IBrowserSessionManager browserSessionManager,
            ILogger<CrawlDataActivity> logger)
        {
            _browserSessionManager = browserSessionManager;
            _logger = logger;
        }

        /// <summary>
        /// The CSS selector to identify elements to extract data from
        /// </summary>
        public string Selector { get; set; } = default!;

        /// <summary>
        /// The property to extract from the matched elements (e.g., innerText, textContent, href, value, etc.)
        /// </summary>
        public string PropertyToExtract { get; set; } = "innerText";

        /// <summary>
        /// When true, extracts data from all matching elements. When false, extracts only from the first matching element.
        /// </summary>
        public bool ExtractAll { get; set; } = false;

        /// <summary>
        /// Timeout in milliseconds to wait for the selector to appear
        /// </summary>
        public int Timeout { get; set; } = 30000;

        /// <summary>
        /// The name of the output variable that will contain the extracted data
        /// </summary>
        public string OutputVariableName { get; set; } = "ExtractedData";

        /// <summary>
        /// If true, uses strict mode for element selection which throws if multiple elements match selector
        /// </summary>
        public bool StrictMode { get; set; } = false;

        /// <summary>
        /// If true, waits for content to be present before extracting
        /// </summary>
        public bool WaitForContentPresent { get; set; } = true;

        /// <summary>
        /// If true, enables DOM content to be fully loaded before extracting
        /// </summary>
        public bool WaitForDomContentLoaded { get; set; } = true;

        /// <summary>
        /// Maximum character length for logging extracted content (to avoid excessive logging)
        /// </summary>
        public int MaxLogLength { get; set; } = 200;

        /// <summary>
        /// Execute the activity
        /// </summary>
        protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
        {
            var workflowInstanceId = context.WorkflowExecutionContext.Id.ToString();
            var selector = Selector;
            var propertyToExtract = PropertyToExtract;
            var extractAll = ExtractAll;
            var timeout = Timeout;
            var strictMode = StrictMode;

            _logger.LogInformation("Executing CrawlDataActivity for workflow {WorkflowInstanceId} with selector {Selector}", 
                workflowInstanceId, selector);

            try
            {
                // Get the page
                var page = await _browserSessionManager.GetOrCreatePageForWorkflowAsync(workflowInstanceId, context.CancellationToken);
                
                // Wait for page to be ready
                if (WaitForDomContentLoaded)
                {
                    _logger.LogDebug("Waiting for DOM content to be loaded...");
                    await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
                }
                
                // Create locator - Note: Playwright .NET doesn't support 'strict' option in PageLocatorOptions
                // We'll handle this ourselves if needed
                var locator = page.Locator(selector);
                
                // Check if locator matches multiple elements and we're in strict mode
                if (strictMode)
                {
                    var count = await locator.CountAsync();
                    if (count > 1 && !extractAll)
                    {
                        throw new InvalidOperationException($"Strict mode enabled but selector '{selector}' matches {count} elements");
                    }
                }
                
                // Wait for element count if extracting all
                if (extractAll)
                {
                    int count = await locator.CountAsync();
                    _logger.LogDebug("Found {Count} elements matching selector {Selector}", count, selector);
                    
                    if (count == 0)
                    {
                        _logger.LogWarning("No elements found with selector: {Selector}", selector);
                        context.SetVariable(OutputVariableName, new List<string>());
                        context.WorkflowExecutionContext.Output[OutputVariableName] = new List<string>();
                        await context.CompleteActivityAsync();
                        return;
                    }
                }
                else
                {
                    // Check if element exists
                    var elementExists = await locator.CountAsync() > 0;
                    if (!elementExists)
                    {
                        _logger.LogWarning("No element found with selector: {Selector}", selector);
                        context.SetVariable(OutputVariableName, string.Empty);
                        context.WorkflowExecutionContext.Output[OutputVariableName] = string.Empty;
                        await context.CompleteActivityAsync();
                        return;
                    }
                }
                
                // Wait for content to be present if specified
                if (WaitForContentPresent)
                {
                    try
                    {
                        _logger.LogDebug("Waiting for content to be present in selector {Selector}...", selector);
                        await locator.First.WaitForAsync(new LocatorWaitForOptions
                        {
                            State = WaitForSelectorState.Visible,
                            Timeout = timeout
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Timeout waiting for content to be present in selector {Selector}", selector);
                    }
                }
                
                // Extract the data using appropriate method based on property
                object extractedData;
                
                if (extractAll)
                {
                    var values = new List<string>();
                    var count = await locator.CountAsync();
                    
                    for (int i = 0; i < count; i++)
                    {
                        var element = locator.Nth(i);
                        string value = await GetPropertyValueAsync(element, propertyToExtract);
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            values.Add(value);
                        }
                    }
                    
                    extractedData = values;
                    
                    // Log the number of values extracted with a preview
                    string preview = values.Count > 0 
                        ? string.Join(", ", values.Take(3).Select(v => v.Length > 30 ? v.Substring(0, 30) + "..." : v))
                        : "empty array";
                    
                    if (values.Count > 3)
                    {
                        preview += $"... and {values.Count - 3} more items";
                    }
                    
                    _logger.LogDebug("Extracted {Count} values: [{Preview}]", values.Count, preview);
                }
                else
                {
                    string value = await GetPropertyValueAsync(locator.First, propertyToExtract);
                    extractedData = value;
                    
                    // Log the extracted value with length limitation
                    string logValue = value;
                    if (logValue.Length > MaxLogLength)
                    {
                        logValue = logValue.Substring(0, MaxLogLength) + "...";
                    }
                    
                    _logger.LogDebug("Extracted value: {Value}", logValue);
                }
                
                // Set variable for access in other activities
                context.SetVariable(OutputVariableName, extractedData);
                
                // Also set as workflow output to ensure it's preserved
                context.WorkflowExecutionContext.Output[OutputVariableName] = extractedData;
                
                _logger.LogInformation("Successfully extracted data from selector {Selector}", selector);
                
                // Complete the activity
                await context.CompleteActivityAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to extract data for selector {Selector}: {Message}", selector, ex.Message);
                
                // Create a special output for errors to help with debugging
                var errorInfo = new {
                    Error = ex.Message,
                    Selector = selector,
                    Property = propertyToExtract,
                    TimeStamp = DateTime.UtcNow
                };
                
                context.SetVariable($"{OutputVariableName}_Error", errorInfo);
                context.WorkflowExecutionContext.Output[$"{OutputVariableName}_Error"] = errorInfo;
                
                // Rethrow to properly signal activity failure
                throw;
            }
        }
        
        /// <summary>
        /// Gets the value of a property from an element
        /// </summary>
        private async Task<string> GetPropertyValueAsync(ILocator locator, string propertyName)
        {
            try
            {
                // Handle special property names differently
                switch (propertyName.ToLowerInvariant())
                {
                    case "innertext":
                        return await locator.InnerTextAsync();
                        
                    case "textcontent":
                        return await locator.TextContentAsync() ?? string.Empty;
                        
                    case "innerhtml":
                        return await locator.InnerHTMLAsync();
                        
                    case "outerhtml":
                        return await locator.EvaluateAsync<string>("el => el.outerHTML");
                        
                    default:
                        // Try to get attribute first
                        string attributeValue = await locator.GetAttributeAsync(propertyName);
                        if (attributeValue != null)
                        {
                            return attributeValue;
                        }
                        
                        // Fall back to property access via JavaScript
                        return await locator.EvaluateAsync<string>($"el => {{ try {{ return String(el['{propertyName}']) }} catch {{ return '' }} }}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting property {PropertyName}: {Message}", propertyName, ex.Message);
                return string.Empty;
            }
        }
    }
} 