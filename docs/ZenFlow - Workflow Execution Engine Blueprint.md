# ZenFlow Workflow Execution Engine

## 1. Architecture Overview

The ZenFlow Workflow Execution Engine is built on Elsa Workflows for workflow management, with Playwright integration for browser automation workflows. This architecture supports both sequential and conditional execution flows:

```
ZenFlow Workflow Engine
├── Elsa Workflow Engine Integration
│   ├── Workflow Definition Service
│   ├── Workflow Instance Manager
│   ├── Workflow Registry
│   └── Workflow Storage Provider
├── Browser Automation Layer
│   ├── Browser Session Manager
│   ├── Custom Browser Automation Activities
│   └── Playwright Integration Service
├── Execution Services
│   ├── Variable Context Manager
│   └── Execution Persistence Service
└── User Interface Components
    ├── Workflow Designer Integration
    └── Execution Monitoring Dashboard
```

## 2. Core Components

### 2.1 Elsa Workflows Integration

ZenFlow leverages Elsa Workflows 3.0 as its core workflow engine, providing:

- Workflow definition and execution framework
- Activity library infrastructure
- Workflow persistence
- Visual designer capabilities

```csharp
public static class ElsaIntegrationExtensions
{
    public static IServiceCollection AddElsaWorkflows(this IServiceCollection services, Action<ElsaOptionsBuilder> configureElsa)
    {
        // Add Elsa services
        services.AddElsa(configureElsa);
        
        // Add ZenFlow browser automation activities
        services.AddZenFlowActivities();
        
        return services;
    }
    
    public static IServiceCollection AddZenFlowActivities(this IServiceCollection services)
    {
        // Register browser automation activities
        services.AddActivity<NavigateActivity>()
               .AddActivity<ClickActivity>()
               .AddActivity<InputTextActivity>()
               .AddActivity<ScreenshotActivity>()
               .AddActivity<WaitForSelectorActivity>()
               .AddActivity<ExtractDataActivity>();
        
        return services;
    }
}
```

### 2.2 Browser Session Manager

Manages Playwright browser instances, adapted to integrate with Elsa workflow contexts:

```csharp
public class BrowserSessionManager : IBrowserSessionManager, IAsyncDisposable
{
    private readonly Dictionary<string, IBrowser> _browsers = new();
    private readonly Dictionary<string, IPage> _pages = new();
    private readonly SemaphoreSlim _semaphore = new(1);
    private readonly IBrowsersLauncher _browsersLauncher;
    private readonly ILogger<BrowserSessionManager> _logger;

    public BrowserSessionManager(IBrowsersLauncher browsersLauncher, ILogger<BrowserSessionManager> logger)
    {
        _browsersLauncher = browsersLauncher;
        _logger = logger;
    }

    public async Task<IPage> GetOrCreatePageForWorkflowAsync(string workflowInstanceId, CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            // Return existing page if available
            if (_pages.TryGetValue(workflowInstanceId, out var existingPage))
                return existingPage;

            // Create new browser session if needed
            if (!_browsers.TryGetValue(workflowInstanceId, out var browser))
            {
                browser = await _browsersLauncher.LaunchBrowserAsync(
                    new BrowserLaunchOptions { Headless = true }
                );
                _browsers[workflowInstanceId] = browser;
            }

            // Create new page
            var page = await browser.NewPageAsync();
            _pages[workflowInstanceId] = page;
            
            // Set default timeout
            await page.SetDefaultTimeoutAsync(30000);
            
            return page;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task CleanupWorkflowResourcesAsync(string workflowInstanceId)
    {
        await _semaphore.WaitAsync();
        try
        {
            // Close and dispose page
            if (_pages.TryGetValue(workflowInstanceId, out var page))
            {
                try
                {
                    await page.CloseAsync();
                    await page.DisposeAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error disposing page for workflow {WorkflowInstanceId}", workflowInstanceId);
                }
                _pages.Remove(workflowInstanceId);
            }

            // Close and dispose browser
            if (_browsers.TryGetValue(workflowInstanceId, out var browser))
            {
                try
                {
                    await browser.CloseAsync();
                    await browser.DisposeAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error disposing browser for workflow {WorkflowInstanceId}", workflowInstanceId);
                }
                _browsers.Remove(workflowInstanceId);
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var pageKvp in _pages.ToList())
        {
            await CleanupWorkflowResourcesAsync(pageKvp.Key);
        }
    }
}
```

### 2.3 Browser Automation Activities

Custom Elsa activities for browser automation using Playwright:

#### 2.3.1 Base Browser Activity Class

```csharp
public abstract class BrowserActivity : Activity
{
    private readonly IBrowserSessionManager _browserSessionManager;

    protected BrowserActivity(IBrowserSessionManager browserSessionManager)
    {
        _browserSessionManager = browserSessionManager;
    }

    protected async Task<IPage> GetPageAsync(ActivityExecutionContext context)
    {
        var workflowInstanceId = context.WorkflowExecutionContext.WorkflowInstance.Id;
        return await _browserSessionManager.GetOrCreatePageForWorkflowAsync(workflowInstanceId, context.CancellationToken);
    }

    protected string ReplaceVariables(string input, ActivityExecutionContext context)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return Regex.Replace(input, @"\{\{(\w+)\}\}", match =>
        {
            string varName = match.Groups[1].Value;
            var value = context.GetVariable(varName);
            return value?.ToString() ?? match.Value;
        });
    }
}
```

#### 2.3.2 Navigate Activity

```csharp
[Activity(
    Category = "Browser Automation",
    DisplayName = "Navigate to URL",
    Description = "Navigates the browser to a specified URL.",
    Outcomes = new[] { OutcomeNames.Done }
)]
public class NavigateActivity : BrowserActivity
{
    public NavigateActivity(IBrowserSessionManager browserSessionManager) : base(browserSessionManager)
    {
    }

    [ActivityInput(Hint = "The URL to navigate to.")]
    public string Url { get; set; } = default!;

    [ActivityInput(Hint = "The navigation timeout in milliseconds.", DefaultValue = 60000)]
    public int Timeout { get; set; } = 60000;

    [ActivityInput(
        Hint = "When to consider navigation succeeded.",
        UIHint = ActivityInputUIHints.Dropdown,
        Options = new[] { "load", "domcontentloaded", "networkidle" },
        DefaultValue = "load"
    )]
    public string WaitUntil { get; set; } = "load";

    [ActivityOutput]
    public string ResponseUrl { get; set; } = default!;

    [ActivityOutput]
    public int StatusCode { get; set; }

    [ActivityOutput]
    public string PageTitle { get; set; } = default!;

    protected override async ValueTask<IActivityExecutionResult> OnExecuteAsync(ActivityExecutionContext context)
    {
        var page = await GetPageAsync(context);
        var url = ReplaceVariables(Url, context);

        // Add http:// if not present
        if (!url.StartsWith("http://") && !url.StartsWith("https://"))
        {
            url = "https://" + url;
        }

        // Configure navigation options
        var options = new NavigateOptions
        {
            Timeout = Timeout,
            WaitUntil = WaitUntil switch
            {
                "domcontentloaded" => WaitUntilState.DOMContentLoaded,
                "load" => WaitUntilState.Load,
                "networkidle" => WaitUntilState.NetworkIdle,
                _ => WaitUntilState.Load
            }
        };

        try
        {
            // Navigate to the URL
            var response = await page.GotoAsync(url, options);
            
            if (response != null)
            {
                ResponseUrl = response.Url;
                StatusCode = response.Status;
                PageTitle = await page.TitleAsync();
                
                // Store variables for access in other activities
                context.SetVariable("PageUrl", ResponseUrl);
                context.SetVariable("PageTitle", PageTitle);
                context.SetVariable("LastStatusCode", StatusCode);
                
                if (!response.Ok)
                {
                    context.LogWarning($"Navigation completed with status code {response.Status}");
                }
            }
            else
            {
                context.LogWarning("Navigation resulted in null response");
            }
            
            return Done();
        }
        catch (Exception ex)
        {
            context.LogError(ex, $"Navigation operation failed: {ex.Message}");
            return Outcome("Error", new { ErrorMessage = ex.Message });
        }
    }
}
```

#### 2.3.3 Click Activity

```csharp
[Activity(
    Category = "Browser Automation",
    DisplayName = "Click Element",
    Description = "Clicks on an element in the webpage.",
    Outcomes = new[] { OutcomeNames.Done, "Error" }
)]
public class ClickActivity : BrowserActivity
{
    public ClickActivity(IBrowserSessionManager browserSessionManager) : base(browserSessionManager)
    {
    }

    [ActivityInput(Hint = "The CSS selector of the element to click.")]
    public string Selector { get; set; } = default!;

    [ActivityInput(Hint = "Whether the element needs to be visible to click.", DefaultValue = true)]
    public bool RequireVisible { get; set; } = true;

    [ActivityInput(Hint = "Delay before clicking in milliseconds.", DefaultValue = 0)]
    public int Delay { get; set; } = 0;

    [ActivityInput(Hint = "Force click even if the element is not visible.", DefaultValue = false)]
    public bool Force { get; set; } = false;

    [ActivityInput(Hint = "Delay after clicking in milliseconds.", DefaultValue = 0)]
    public int AfterDelay { get; set; } = 0;

    protected override async ValueTask<IActivityExecutionResult> OnExecuteAsync(ActivityExecutionContext context)
    {
        var page = await GetPageAsync(context);
        var selector = ReplaceVariables(Selector, context);

        try
        {
            // Find the element
            var element = await page.QuerySelectorAsync(selector);
            if (element == null)
            {
                context.LogWarning($"Element not found with selector: {selector}");
                return Outcome("Error", new { ErrorMessage = $"Element not found with selector: {selector}" });
            }

            // Check visibility if required
            if (RequireVisible)
            {
                var isVisible = await element.IsVisibleAsync();
                if (!isVisible)
                {
                    context.LogWarning($"Element with selector '{selector}' is not visible");
                    return Outcome("Error", new { ErrorMessage = $"Element not visible: {selector}" });
                }
            }

            // Optional delay before clicking
            if (Delay > 0)
            {
                await Task.Delay(Delay, context.CancellationToken);
            }

            // Perform the click
            var clickOptions = new ClickOptions
            {
                Force = Force
            };

            await element.ClickAsync(clickOptions);

            // Optional delay after clicking
            if (AfterDelay > 0)
            {
                await Task.Delay(AfterDelay, context.CancellationToken);
            }

            return Done();
        }
        catch (Exception ex)
        {
            context.LogError(ex, $"Click operation failed: {ex.Message}");
            return Outcome("Error", new { ErrorMessage = ex.Message });
        }
    }
}
```

#### 2.3.4 Input Text Activity

```csharp
[Activity(
    Category = "Browser Automation",
    DisplayName = "Input Text",
    Description = "Enters text into an input field.",
    Outcomes = new[] { OutcomeNames.Done, "Error" }
)]
public class InputTextActivity : BrowserActivity
{
    public InputTextActivity(IBrowserSessionManager browserSessionManager) : base(browserSessionManager)
    {
    }

    [ActivityInput(Hint = "The CSS selector of the input element.")]
    public string Selector { get; set; } = default!;

    [ActivityInput(Hint = "The text to input.")]
    public string Text { get; set; } = default!;

    [ActivityInput(Hint = "Whether to clear the field before typing.", DefaultValue = true)]
    public bool ClearFirst { get; set; } = true;

    [ActivityInput(Hint = "Delay between keypresses for human-like typing (0 for instant).", DefaultValue = 0)]
    public int TypeDelay { get; set; } = 0;

    protected override async ValueTask<IActivityExecutionResult> OnExecuteAsync(ActivityExecutionContext context)
    {
        var page = await GetPageAsync(context);
        var selector = ReplaceVariables(Selector, context);
        var text = ReplaceVariables(Text, context);

        try
        {
            // Find the element
            var element = await page.QuerySelectorAsync(selector);
            if (element == null)
            {
                context.LogWarning($"Element not found with selector: {selector}");
                return Outcome("Error", new { ErrorMessage = $"Element not found with selector: {selector}" });
            }

            // Clear the field first if required
            if (ClearFirst)
            {
                await element.FillAsync("");
            }

            // Type the text
            if (TypeDelay > 0)
            {
                // Human-like typing with delay
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
                await element.FillAsync(text);
            }

            return Done();
        }
        catch (Exception ex)
        {
            context.LogError(ex, $"Input operation failed: {ex.Message}");
            return Outcome("Error", new { ErrorMessage = ex.Message });
        }
    }
}
```

#### 2.3.5 Screenshot Activity

```csharp
[Activity(
    Category = "Browser Automation",
    DisplayName = "Take Screenshot",
    Description = "Takes a screenshot of the page or a specific element.",
    Outcomes = new[] { OutcomeNames.Done, "Error" }
)]
public class ScreenshotActivity : BrowserActivity
{
    private readonly IWebHostEnvironment _environment;

    public ScreenshotActivity(IBrowserSessionManager browserSessionManager, IWebHostEnvironment environment) 
        : base(browserSessionManager)
    {
        _environment = environment;
    }

    [ActivityInput(Hint = "Take screenshot of the full page.", DefaultValue = false)]
    public bool FullPage { get; set; } = false;

    [ActivityInput(Hint = "CSS selector of an element to screenshot (optional).")]
    public string? Selector { get; set; }

    [ActivityOutput]
    public string ScreenshotPath { get; set; } = default!;

    protected override async ValueTask<IActivityExecutionResult> OnExecuteAsync(ActivityExecutionContext context)
    {
        var page = await GetPageAsync(context);
        var workflowInstanceId = context.WorkflowExecutionContext.WorkflowInstance.Id;

        try
        {
            byte[] screenshot;
            string screenshotType;

            // Take screenshot of specific element if selector is provided
            if (!string.IsNullOrEmpty(Selector))
            {
                var selector = ReplaceVariables(Selector, context);
                var element = await page.QuerySelectorAsync(selector);
                
                if (element == null)
                {
                    return Outcome("Error", new { ErrorMessage = $"Element not found with selector: {selector}" });
                }
                
                screenshot = await element.ScreenshotAsync();
                screenshotType = "element";
            }
            else
            {
                // Take full page or viewport screenshot
                var options = new PageScreenshotOptions
                {
                    FullPage = FullPage,
                    Type = ScreenshotType.Png
                };
                
                screenshot = await page.ScreenshotAsync(options);
                screenshotType = FullPage ? "fullpage" : "viewport";
            }

            // Save the screenshot and get the path
            ScreenshotPath = SaveScreenshot(screenshot, workflowInstanceId, screenshotType);
            
            // Make path available as variable
            context.SetVariable("LastScreenshotPath", ScreenshotPath);
            
            return Done();
        }
        catch (Exception ex)
        {
            context.LogError(ex, $"Screenshot operation failed: {ex.Message}");
            return Outcome("Error", new { ErrorMessage = ex.Message });
        }
    }

    private string SaveScreenshot(byte[] screenshot, string workflowInstanceId, string type)
    {
        // Create directory for screenshots if it doesn't exist
        var screenshotDir = Path.Combine(_environment.ContentRootPath, "wwwroot", "screenshots", workflowInstanceId);
        Directory.CreateDirectory(screenshotDir);
        
        // Generate unique filename
        var filename = $"{DateTime.UtcNow:yyyyMMdd_HHmmss}_{type}.png";
        var path = Path.Combine(screenshotDir, filename);
        
        // Save the screenshot
        File.WriteAllBytes(path, screenshot);
        
        // Return the relative path for the client
        return $"/screenshots/{workflowInstanceId}/{filename}";
    }
}
```

#### 2.3.6 Extract Data Activity

```csharp
[Activity(
    Category = "Browser Automation",
    DisplayName = "Extract Data",
    Description = "Extracts data from elements on the page.",
    Outcomes = new[] { OutcomeNames.Done, "Error" }
)]
public class ExtractDataActivity : BrowserActivity
{
    public ExtractDataActivity(IBrowserSessionManager browserSessionManager) : base(browserSessionManager)
    {
    }

    [ActivityInput(Hint = "The CSS selector to match elements.")]
    public string Selector { get; set; } = default!;

    [ActivityInput(
        Hint = "The property to extract (innerText, textContent, innerHTML, value, etc.).",
        UIHint = ActivityInputUIHints.Dropdown,
        Options = new[] { "innerText", "textContent", "innerHTML", "value", "href", "src", "id", "className" },
        DefaultValue = "innerText"
    )]
    public string PropertyToExtract { get; set; } = "innerText";

    [ActivityInput(Hint = "Variable name to store the extracted data.", DefaultValue = "extractedData")]
    public string OutputVariableName { get; set; } = "extractedData";

    [ActivityInput(Hint = "Extract from all matching elements (as array).", DefaultValue = false)]
    public bool ExtractAll { get; set; } = false;

    [ActivityOutput]
    public object ExtractedData { get; set; } = default!;

    protected override async ValueTask<IActivityExecutionResult> OnExecuteAsync(ActivityExecutionContext context)
    {
        var page = await GetPageAsync(context);
        var selector = ReplaceVariables(Selector, context);

        try
        {
            if (ExtractAll)
            {
                // Extract from all matching elements
                var data = await page.EvaluateAsync<string[]>($@"
                    Array.from(document.querySelectorAll('{selector}')).map(el => {{
                        return el.{PropertyToExtract} || '';
                    }})
                ");
                ExtractedData = data;
            }
            else
            {
                // Extract from first matching element
                var element = await page.QuerySelectorAsync(selector);
                if (element == null)
                {
                    context.LogWarning($"Element not found with selector: {selector}");
                    return Outcome("Error", new { ErrorMessage = $"Element not found with selector: {selector}" });
                }

                ExtractedData = await element.EvaluateAsync<string>($"el => el.{PropertyToExtract} || ''");
            }

            // Store extracted data in the specified variable
            context.SetVariable(OutputVariableName, ExtractedData);
            
            return Done();
        }
        catch (Exception ex)
        {
            context.LogError(ex, $"Data extraction failed: {ex.Message}");
            return Outcome("Error", new { ErrorMessage = ex.Message });
        }
    }
}
```

### 2.4 Workflow Cleanup and Lifecycle Management

```csharp
public class WorkflowCleanupHandler : IWorkflowLifecycleHandler
{
    private readonly IBrowserSessionManager _browserSessionManager;
    private readonly ILogger<WorkflowCleanupHandler> _logger;

    public WorkflowCleanupHandler(IBrowserSessionManager browserSessionManager, ILogger<WorkflowCleanupHandler> logger)
    {
        _browserSessionManager = browserSessionManager;
        _logger = logger;
    }

    public async Task WorkflowCompletedAsync(WorkflowCompletedContext context, CancellationToken cancellationToken)
    {
        await CleanupResourcesAsync(context.WorkflowInstance.Id);
    }

    public async Task WorkflowFaultedAsync(WorkflowFaultedContext context, CancellationToken cancellationToken)
    {
        await CleanupResourcesAsync(context.WorkflowInstance.Id);
    }

    public async Task WorkflowCancelledAsync(WorkflowCancelledContext context, CancellationToken cancellationToken)
    {
        await CleanupResourcesAsync(context.WorkflowInstance.Id);
    }

    private async Task CleanupResourcesAsync(string workflowInstanceId)
    {
        try
        {
            await _browserSessionManager.CleanupWorkflowResourcesAsync(workflowInstanceId);
            _logger.LogInformation("Cleaned up browser resources for workflow {workflowInstanceId}", workflowInstanceId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up browser resources for workflow {workflowInstanceId}", workflowInstanceId);
        }
    }
}
```

## 3. Implementation Guide

### 3.1 Dependency Injection Registration

```csharp
public static IServiceCollection AddZenFlowServices(this IServiceCollection services, IConfiguration configuration)
{
    // Add Elsa core services with ZenFlow customizations
    services.AddElsa(elsa => elsa
        .AddConsoleActivities()
        .AddHttpActivities()
        .AddQuartzTemporalActivities()
        .AddWorkflowsFrom<Startup>()
        // Configure workflow persistence
        .UseEntityFrameworkPersistence(ef =>
        {
            ef.UseSqlServer(configuration.GetConnectionString("ElsaDb"));
        })
        // Add workflow context providers
        .AddWorkflowContextProvider<BrowserWorkflowContextProvider>()
        // Add custom JavaScript engine for expressions
        .UseJavaScriptExpressions()
        // Add server integration
        .AddElsaApiEndpoints()
    );
    
    // Add ZenFlow browser automation activities
    services.AddZenFlowActivities();
    
    // Add Playwright services
    services.AddSingleton<IBrowsersLauncher, BrowsersLauncher>();
    services.AddSingleton<IBrowserSessionManager, BrowserSessionManager>();
    
    // Add workflow lifecycle handlers
    services.AddScoped<IWorkflowLifecycleHandler, WorkflowCleanupHandler>();
    
    return services;
}
```

### 3.2 Workflow API Controller

```csharp
[ApiController]
[Route("api/workflows")]
[Authorize]
public class WorkflowsController : ControllerBase
{
    private readonly IWorkflowRegistry _workflowRegistry;
    private readonly IWorkflowDefinitionStore _workflowDefinitionStore;
    private readonly IWorkflowRunner _workflowRunner;
    private readonly IWorkflowInstanceStore _workflowInstanceStore;
    private readonly ICurrentUserService _currentUserService;

    public WorkflowsController(
        IWorkflowRegistry workflowRegistry,
        IWorkflowDefinitionStore workflowDefinitionStore,
        IWorkflowRunner workflowRunner,
        IWorkflowInstanceStore workflowInstanceStore,
        ICurrentUserService currentUserService)
    {
        _workflowRegistry = workflowRegistry;
        _workflowDefinitionStore = workflowDefinitionStore;
        _workflowRunner = workflowRunner;
        _workflowInstanceStore = workflowInstanceStore;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<WorkflowDefinitionSummary>>> GetWorkflows(CancellationToken cancellationToken)
    {
        // Filter by current user
        var userId = _currentUserService.UserId;
        var definitions = await _workflowDefinitionStore.FindManyAsync(
            x => x.CustomAttributes.GetValueOrDefault("CreatedBy") == userId, 
            cancellationToken: cancellationToken);
            
        return Ok(definitions.Select(x => new WorkflowDefinitionSummary
        {
            Id = x.Id,
            Name = x.Name,
            Description = x.Description,
            Version = x.Version,
            IsLatest = x.IsLatest,
            IsPublished = x.IsPublished,
            CreatedAt = x.CreatedAt
        }));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<WorkflowDefinition>> GetWorkflow(string id, CancellationToken cancellationToken)
    {
        var definition = await _workflowDefinitionStore.FindByIdAsync(id, VersionOptions.Latest, cancellationToken);
        if (definition == null)
            return NotFound();
            
        // Security check
        var createdBy = definition.CustomAttributes.GetValueOrDefault("CreatedBy");
        if (createdBy != _currentUserService.UserId)
            return Forbid();
            
        return Ok(definition);
    }

    [HttpPost("{id}/execute")]
    public async Task<ActionResult<WorkflowExecutionResponse>> ExecuteWorkflow(
        string id, 
        [FromBody] Dictionary<string, object>? input, 
        CancellationToken cancellationToken)
    {
        var definition = await _workflowDefinitionStore.FindByIdAsync(id, VersionOptions.Latest, cancellationToken);
        if (definition == null)
            return NotFound();
            
        // Security check
        var createdBy = definition.CustomAttributes.GetValueOrDefault("CreatedBy");
        if (createdBy != _currentUserService.UserId)
            return Forbid();
        
        // Execute workflow
        var startInput = new WorkflowInput(input ?? new Dictionary<string, object>());
        var result = await _workflowRunner.RunWorkflowAsync(definition, startInput, cancellationToken);
        
        return Ok(new WorkflowExecutionResponse
        {
            WorkflowInstanceId = result.WorkflowInstance.Id,
            Status = result.WorkflowInstance.Status.ToStringValue(),
            Completed = result.WorkflowInstance.Status == WorkflowStatus.Finished
        });
    }

    [HttpGet("instances/{instanceId}")]
    public async Task<ActionResult<WorkflowInstance>> GetWorkflowInstance(string instanceId, CancellationToken cancellationToken)
    {
        var instance = await _workflowInstanceStore.FindByIdAsync(instanceId, cancellationToken);
        if (instance == null)
            return NotFound();
            
        // Get workflow definition to check permissions
        var definition = await _workflowDefinitionStore.FindByIdAsync(
            instance.DefinitionId, 
            VersionOptions.SpecificVersion(instance.Version), 
            cancellationToken);
            
        if (definition == null)
            return NotFound();
            
        // Security check
        var createdBy = definition.CustomAttributes.GetValueOrDefault("CreatedBy");
        if (createdBy != _currentUserService.UserId)
            return Forbid();
            
        return Ok(instance);
    }

    public class WorkflowExecutionResponse
    {
        public string WorkflowInstanceId { get; set; } = default!;
        public string Status { get; set; } = default!;
        public bool Completed { get; set; }
    }

    public class WorkflowDefinitionSummary
    {
        public string Id { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public int Version { get; set; }
        public bool IsLatest { get; set; }
        public bool IsPublished { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
```

### 3.3 Creating Custom Workflows Programmatically

Example of creating a browser automation workflow programmatically:

```csharp
public class SampleBrowserWorkflow : IWorkflow
{
    public void Build(IWorkflowBuilder builder)
    {
        builder
            .WithCustomAttribute("CreatedBy", "system")
            .WithDisplayName("Sample Browser Automation")
            .WithDescription("A sample browser automation workflow")
            .WithVersion(1)
            .WithPersistenceBehavior(WorkflowPersistenceBehavior.Suspended)
            .WithDeleteCompletedInstances(false);

        // Define the workflow activities
        builder
            .StartWith<NavigateActivity>(activity => activity
                .Set(a => a.Url, "https://www.example.com")
                .Set(a => a.WaitUntil, "networkidle"))
            .Then<ScreenshotActivity>(activity => activity
                .Set(a => a.FullPage, true))
            .Then<InputTextActivity>(activity => activity
                .Set(a => a.Selector, "#search")
                .Set(a => a.Text, "Elsa Workflows")
                .Set(a => a.TypeDelay, 50))
            .Then<ClickActivity>(activity => activity
                .Set(a => a.Selector, "#searchButton"))
            .Then<WaitForSelectorActivity>(activity => activity
                .Set(a => a.Selector, ".results")
                .Set(a => a.Timeout, 10000))
            .Then<ExtractDataActivity>(activity => activity
                .Set(a => a.Selector, ".results .title")
                .Set(a => a.PropertyToExtract, "innerText")
                .Set(a => a.ExtractAll, true)
                .Set(a => a.OutputVariableName, "searchResults"))
            .Then<ScreenshotActivity>(activity => activity
                .Set(a => a.FullPage, true));
    }
}
```

## 4. Component Integration

### 4.1 Integrating with Elsa Designer

```csharp
// Startup.cs
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    // ... other middleware configuration
    
    // Serve static files for the workflow designer
    app.UseStaticFiles();
    
    // Map Elsa Dashboard & Designer endpoints
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();
    
    app.UseEndpoints(endpoints =>
    {
        // Map API endpoints
        endpoints.MapControllers();
        
        // Map Elsa Designer + API
        endpoints.MapFallbackToPage("/_Host");
    });
}
```

```html
@* Pages/_Host.cshtml *@
@page "/_Host"
@{
    var serverUrl = $"{Request.Scheme}://{Request.Host}";
}
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>ZenFlow - Workflow Designer</title>
    <link rel="icon" type="image/png" href="/_content/Elsa.Designer.Components.Web/elsa-workflows-studio/assets/images/favicon-32x32.png" sizes="32x32">
    <link rel="stylesheet" href="/_content/Elsa.Designer.Components.Web/elsa-workflows-studio/assets/fonts/inter/inter.css">
    <link rel="stylesheet" href="/_content/Elsa.Designer.Components.Web/elsa-workflows-studio/elsa-workflows-studio.css">
    <script src="/_content/Elsa.Designer.Components.Web/monaco-editor/min/vs/loader.js"></script>
    <script type="module" src="/_content/Elsa.Designer.Components.Web/elsa-workflows-studio/elsa-workflows-studio.esm.js"></script>
</head>
<body>
    <elsa-studio-root server-url="@serverUrl" monaco-lib-path="/_content/Elsa.Designer.Components.Web/monaco-editor/min">
        <elsa-studio-dashboard></elsa-studio-dashboard>
    </elsa-studio-root>
</body>
</html>
```

### 4.2 Custom Activity UI Providers

```typescript
// Custom UI provider for browser automation activities
import { ElsaPlugin } from '@elsa-workflows/elsa-workflows-studio';

export class BrowserAutomationActivitiesPlugin implements ElsaPlugin {
  setup(elsaStudio) {
    const { activityDescriptors } = elsaStudio;

    // Register custom icon providers
    activityDescriptors.register({
      displayName: 'NavigateActivity',
      icon: 'fas fa-globe',
      type: 'NavigateActivity'
    });
    
    activityDescriptors.register({
      displayName: 'ClickActivity',
      icon: 'fas fa-mouse-pointer',
      type: 'ClickActivity'
    });
    
    activityDescriptors.register({
      displayName: 'InputTextActivity',
      icon: 'fas fa-keyboard',
      type: 'InputTextActivity'
    });
    
    activityDescriptors.register({
      displayName: 'ScreenshotActivity',
      icon: 'fas fa-camera',
      type: 'ScreenshotActivity'
    });
    
    activityDescriptors.register({
      displayName: 'ExtractDataActivity',
      icon: 'fas fa-screwdriver',
      type: 'ExtractDataActivity'
    });
  }
}
```

## 5. Implementation Plan

### Phase 1: Elsa Core Integration
- Set up Elsa Workflows in the project
- Configure persistence with SQL Server 
- Create basic API endpoints for workflow management
- Implement user-based workflow security

### Phase 2: Browser Automation Activities
- Integrate Playwright with Elsa
- Create browser session management
- Implement custom browser automation activities:
  - Navigation
  - Click actions
  - Input text
  - Screenshots
  - Data extraction

### Phase 3: Designer Integration
- Set up Elsa workflow designer
- Customize activity UI components
- Create custom activity designers

### Phase 4: Error Handling and Monitoring
- Implement error handling and retries
- Create workflow execution history
- Build a monitoring dashboard

### Phase 5: Advanced Features
- Add parallel execution support
- Implement workflow scheduling
- Create more advanced browser automation activities
- Add proxy and authentication support

## 6. Advantages of Elsa Integration

1. **Reduced Development Time**: Leveraging Elsa's existing workflow engine eliminates the need to build core workflow components from scratch.

2. **Visual Designer**: Elsa provides a web-based workflow designer out of the box, enabling non-technical users to create and edit workflows.

3. **Persistence and State Management**: Elsa handles workflow persistence, resumption, and state management automatically.

4. **Expression Support**: Built-in support for C#, JavaScript, and Liquid expressions for workflow logic.

5. **Extensibility**: Elsa's activity model makes it easy to add custom browser automation activities while leveraging the existing workflow infrastructure.

6. **Community and Documentation**: Benefit from Elsa's active community, documentation, and regular updates.

7. **Scalability**: Elsa is designed to be scalable, with support for distributed execution and persistence in external databases.