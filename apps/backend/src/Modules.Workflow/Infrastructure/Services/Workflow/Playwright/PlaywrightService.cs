using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Modules.Workflow.Domain.Interfaces.Core;

namespace Modules.Workflow.Infrastructure.Services.Playwright
{
    public class PlaywrightService : IPlaywrightService
    {
        private readonly ILogger<PlaywrightService> _logger;
        private readonly IPlaywrightFactory _playwrightFactory;

        public PlaywrightService(ILogger<PlaywrightService> logger, IPlaywrightFactory playwrightFactory)
        {
            _logger = logger;
            _playwrightFactory = playwrightFactory;
        }

        public async Task<string> GetElementAttributeAsync(string url, string selector, string attributeName)
        {
            _logger.LogInformation("Getting attribute {AttributeName} from element {Selector} on page {Url}", 
                attributeName, selector, url);
                
            using var playwright = await _playwrightFactory.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true
            });

            var page = await browser.NewPageAsync();
            
            try
            {
                await page.GotoAsync(url, new PageGotoOptions
                {
                    WaitUntil = WaitUntilState.NetworkIdle,
                    Timeout = 30000
                });
                
                var element = await page.QuerySelectorAsync(selector);
                if (element == null)
                {
                    _logger.LogWarning("Element with selector {Selector} not found on page {Url}", selector, url);
                    return null;
                }

                var attributeValue = await element.GetAttributeAsync(attributeName);
                
                _logger.LogInformation("Successfully retrieved attribute {AttributeName} with value {AttributeValue}", 
                    attributeName, attributeValue);
                    
                return attributeValue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting attribute {AttributeName} from element {Selector} on page {Url}", 
                    attributeName, selector, url);
                throw;
            }
            finally
            {
                await page.CloseAsync();
            }
        }

        public async Task<string> ExtractTextFromElementAsync(string url, string selector)
        {
            _logger.LogInformation("Extracting text from element {Selector} on page {Url}", selector, url);
                
            using var playwright = await _playwrightFactory.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true
            });

            var page = await browser.NewPageAsync();
            
            try
            {
                await page.GotoAsync(url, new PageGotoOptions
                {
                    WaitUntil = WaitUntilState.NetworkIdle,
                    Timeout = 30000
                });
                
                var element = await page.QuerySelectorAsync(selector);
                if (element == null)
                {
                    _logger.LogWarning("Element with selector {Selector} not found on page {Url}", selector, url);
                    return null;
                }

                var textContent = await element.TextContentAsync();
                
                _logger.LogInformation("Successfully extracted text from element {Selector} (length: {TextLength})", 
                    selector, textContent?.Length ?? 0);
                    
                return textContent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting text from element {Selector} on page {Url}", 
                    selector, url);
                throw;
            }
            finally
            {
                await page.CloseAsync();
            }
        }
    }
}