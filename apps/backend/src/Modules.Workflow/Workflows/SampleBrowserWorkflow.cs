using Elsa.Workflows;
using Elsa.Workflows.Activities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Modules.Workflow.DDD.Interfaces;
using Modules.Workflow.Services.BrowserAutomation.Activities;
using System.Collections.Generic;

namespace Modules.Workflow.Workflows
{
    /// <summary>
    /// Configuration options for the sample browser automation workflow
    /// </summary>
    public class SampleBrowserWorkflowOptions
    {
        /// <summary>
        /// The URL of the initial page to navigate to
        /// </summary>
        public string InitialUrl { get; set; } = "https://www.example.com";
        
        /// <summary>
        /// The search engine URL
        /// </summary>
        public string SearchEngineUrl { get; set; } = "https://www.google.com";
        
        /// <summary>
        /// The search term to use
        /// </summary>
        public string SearchTerm { get; set; } = "Elsa Workflows";
        
        /// <summary>
        /// The selector for the search input box
        /// </summary>
        public string SearchInputSelector { get; set; } = "textarea[name='q']";
        
        /// <summary>
        /// The selector for the search button
        /// </summary>
        public string SearchButtonSelector { get; set; } = "textarea[name='btnK']";
        
        /// <summary>
        /// The selector for search results
        /// </summary>
        public string SearchResultsSelector { get; set; } = "div[role='gridcell']";
        
        /// <summary>
        /// The selector for extracting search result data
        /// </summary>
        public string ExtractDataSelector { get; set; } = "div[role='gridcell']";
        
        /// <summary>
        /// Whether to take screenshots during workflow execution
        /// </summary>
        public bool EnableScreenshots { get; set; } = false;
        
        /// <summary>
        /// The delay between keystrokes when typing (ms)
        /// </summary>
        public int TypeDelay { get; set; } = 50;
    }

    /// <summary>
    /// A sample workflow that demonstrates browser automation
    /// </summary>
    public class SampleBrowserWorkflow : WorkflowBase
    {
        private readonly IBrowserSessionManager _browserSessionManager;
        private readonly Microsoft.AspNetCore.Hosting.IWebHostEnvironment _environment;
        private readonly ILoggerFactory _loggerFactory;
        private readonly SampleBrowserWorkflowOptions _options;

        public SampleBrowserWorkflow(
            IBrowserSessionManager browserSessionManager,
            Microsoft.AspNetCore.Hosting.IWebHostEnvironment environment,
            ILoggerFactory loggerFactory,
            IOptions<SampleBrowserWorkflowOptions> options = null)
        {
            _browserSessionManager = browserSessionManager;
            _environment = environment;
            _loggerFactory = loggerFactory;
            _options = options?.Value ?? new SampleBrowserWorkflowOptions();
        }

        protected override void Build(IWorkflowBuilder builder)
        {
            // Create a list to hold our workflow activities
            var activities = new List<IActivity>();
            
            // Add initial navigation
            activities.Add(CreateNavigateActivity(_options.InitialUrl));
            
            // Take a screenshot if enabled
            if (_options.EnableScreenshots)
            {
                activities.Add(CreateScreenshotActivity(true));
            }
            
            // Navigate to search engine
            activities.Add(CreateNavigateActivity(_options.SearchEngineUrl));
            
            // Perform search
            activities.Add(CreateInputTextActivity(
                _options.SearchInputSelector, 
                _options.SearchTerm, 
                _options.TypeDelay));
            
            // Click search button
            activities.Add(CreateClickActivity(_options.SearchButtonSelector));
            
            // Wait for search results
            activities.Add(CreateWaitForSelectorActivity(_options.SearchResultsSelector));
            
            // Extract search results
            activities.Add(CreateExtractDataActivity(
                _options.ExtractDataSelector,
                "innerText",
                true,
                "searchResults"));
            
            // Take a final screenshot if enabled
            if (_options.EnableScreenshots)
            {
                activities.Add(CreateScreenshotActivity(true));
            }

            // Build the sequence of activities
            builder.Root = new Sequence
            {
                Activities = activities
            };
        }

        // Factory methods to create activity instances with their required dependencies

        private NavigateActivity CreateNavigateActivity(string url)
        {
            return new NavigateActivity(
                _browserSessionManager, 
                _loggerFactory.CreateLogger<NavigateActivity>())
            {
                Url = url
            };
        }

        private ScreenshotActivity CreateScreenshotActivity(bool fullPage)
        {
            return new ScreenshotActivity(
                _browserSessionManager, 
                _environment, 
                _loggerFactory.CreateLogger<ScreenshotActivity>())
            {
                FullPage = fullPage
            };
        }

        private InputTextActivity CreateInputTextActivity(string selector, string text, int typeDelay = 0)
        {
            return new InputTextActivity(
                _browserSessionManager, 
                _loggerFactory.CreateLogger<InputTextActivity>())
            {
                Selector = selector,
                Text = text,
                TypeDelay = typeDelay
            };
        }

        private ClickActivity CreateClickActivity(string selector)
        {
            return new ClickActivity(
                _browserSessionManager, 
                _loggerFactory.CreateLogger<ClickActivity>())
            {
                Selector = selector
            };
        }

        private WaitForSelectorActivity CreateWaitForSelectorActivity(string selector, int timeout = 30000)
        {
            return new WaitForSelectorActivity(
                _browserSessionManager, 
                _loggerFactory.CreateLogger<WaitForSelectorActivity>())
            {
                Selector = selector,
                Timeout = timeout
            };
        }

        private ExtractDataActivity CreateExtractDataActivity(
            string selector, 
            string propertyToExtract = "innerText", 
            bool extractAll = false, 
            string outputVariableName = "extractedData")
        {
            return new ExtractDataActivity(
                _browserSessionManager, 
                _loggerFactory.CreateLogger<ExtractDataActivity>())
            {
                Selector = selector,
                PropertyToExtract = propertyToExtract,
                ExtractAll = extractAll,
                OutputVariableName = outputVariableName
            };
        }
    }
} 