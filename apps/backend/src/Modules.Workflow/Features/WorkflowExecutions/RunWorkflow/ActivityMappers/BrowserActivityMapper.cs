using Elsa.Workflows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Modules.Workflow.Infrastructure.Services.BrowserAutomation.Activities;

namespace Modules.Workflow.Features.WorkflowExecutions.RunWorkflow.ActivityMappers
{
    /// <summary>
    /// Maps browser automation related activity types to their respective activity instances
    /// </summary>
    public class BrowserActivityMapper : IActivityMapper
    {
        private readonly ILogger<BrowserActivityMapper> _logger;

        public BrowserActivityMapper(ILogger<BrowserActivityMapper> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Determines if this mapper can handle the specified activity type
        /// </summary>
        public bool CanMap(string activityType)
        {
            return activityType switch
            {
                "navigate" => true,
                "click" => true,
                "inputtext" => true,
                "screenshot" => true,
                "crawldata" => true,
                "extractdata" => true,
                "waitforselector" => true,
                "manualtrigger" => true,
                _ => false
            };
        }

        /// <summary>
        /// Maps a browser-related activity configuration to an activity instance
        /// </summary>
        public IActivity MapToActivity(string activityType, Dictionary<string, object> config, IServiceProvider serviceProvider)
        {
            _logger.LogDebug("Mapping browser activity type: {ActivityType}", activityType);

            return activityType switch
            {
                "navigate" => CreateNavigateActivity(config, serviceProvider),
                "click" => CreateClickActivity(config, serviceProvider),
                "inputtext" => CreateInputTextActivity(config, serviceProvider),
                "screenshot" => CreateScreenshotActivity(config, serviceProvider),
                "crawldata" or "extractdata" => CreateCrawlDataActivity(config, serviceProvider),
                "waitforselector" => CreateWaitForSelectorActivity(config, serviceProvider),
                "manualtrigger" => CreateManualTriggerActivity(serviceProvider),
                _ => throw new NotSupportedException($"Browser activity type '{activityType}' is not supported")
            };
        }

        private NavigateActivity CreateNavigateActivity(Dictionary<string, object> config, IServiceProvider serviceProvider)
        {
            var nav = ActivatorUtilities.CreateInstance<NavigateActivity>(serviceProvider);
            nav.Url = config.TryGetValue("url", out var url) ? url?.ToString() ?? string.Empty : string.Empty;
            nav.Timeout = config.TryGetValue("timeout", out var timeout) && int.TryParse(timeout?.ToString(), out var t) ? t : 60000;
            nav.WaitUntil = config.TryGetValue("waitUntil", out var waitUntil) ? waitUntil?.ToString() ?? "load" : "load";
            return nav;
        }

        private ClickActivity CreateClickActivity(Dictionary<string, object> config, IServiceProvider serviceProvider)
        {
            var click = ActivatorUtilities.CreateInstance<ClickActivity>(serviceProvider);
            click.Selector = config.TryGetValue("selector", out var selector) ? selector?.ToString() ?? string.Empty : string.Empty;
            click.RequireVisible = config.TryGetValue("requireVisible", out var requireVisible) && bool.TryParse(requireVisible?.ToString(), out var rv) ? rv : true;
            click.Delay = config.TryGetValue("delay", out var delay) && int.TryParse(delay?.ToString(), out var d) ? d : 0;
            click.Force = config.TryGetValue("force", out var force) && bool.TryParse(force?.ToString(), out var f) ? f : false;
            click.AfterDelay = config.TryGetValue("afterDelay", out var afterDelay) && int.TryParse(afterDelay?.ToString(), out var ad) ? ad : 0;
            return click;
        }

        private InputTextActivity CreateInputTextActivity(Dictionary<string, object> config, IServiceProvider serviceProvider)
        {
            var inputText = ActivatorUtilities.CreateInstance<InputTextActivity>(serviceProvider);
            inputText.Selector = config.TryGetValue("selector", out var sel) ? sel?.ToString() ?? string.Empty : string.Empty;
            inputText.Text = config.TryGetValue("text", out var text) ? text?.ToString() ?? string.Empty : string.Empty;
            inputText.TypeDelay = config.TryGetValue("typeDelay", out var td) && int.TryParse(td?.ToString(), out var tdi) ? tdi : 0;
            inputText.ClearFirst = config.TryGetValue("clearFirst", out var cf) && bool.TryParse(cf?.ToString(), out var cfb) ? cfb : true;
            return inputText;
        }

        private ScreenshotActivity CreateScreenshotActivity(Dictionary<string, object> config, IServiceProvider serviceProvider)
        {
            var screenshot = ActivatorUtilities.CreateInstance<ScreenshotActivity>(serviceProvider);
            screenshot.FullPage = config.TryGetValue("fullPage", out var fp) && bool.TryParse(fp?.ToString(), out var fpb) ? fpb : false;
            screenshot.Selector = config.TryGetValue("selector", out var ss) ? ss?.ToString() : null;
            return screenshot;
        }

        private CrawlDataActivity CreateCrawlDataActivity(Dictionary<string, object> config, IServiceProvider serviceProvider)
        {
            var extract = ActivatorUtilities.CreateInstance<CrawlDataActivity>(serviceProvider);
            extract.Selector = config.TryGetValue("selector", out var es) ? es?.ToString() ?? string.Empty : string.Empty;
            extract.PropertyToExtract = config.TryGetValue("propertyToExtract", out var pte) ? pte?.ToString() ?? "innerText" : "innerText";
            extract.ExtractAll = config.TryGetValue("extractAll", out var ea) && bool.TryParse(ea?.ToString(), out var eab) ? eab : false;
            extract.OutputVariableName = config.TryGetValue("outputVariableName", out var ovn) ? ovn?.ToString() ?? "extractedData" : "extractedData";
            extract.Timeout = config.TryGetValue("timeout", out var cdt) && int.TryParse(cdt?.ToString(), out var cdti) ? cdti : 30000;
            extract.StrictMode = config.TryGetValue("strictMode", out var sm) && bool.TryParse(sm?.ToString(), out var smb) ? smb : false;
            extract.WaitForContentPresent = config.TryGetValue("waitForContentPresent", out var wfcp) && bool.TryParse(wfcp?.ToString(), out var wfcpb) ? wfcpb : true;
            extract.WaitForDomContentLoaded = config.TryGetValue("waitForDomContentLoaded", out var wfdcl) && bool.TryParse(wfdcl?.ToString(), out var wfdclb) ? wfdclb : true;
            extract.MaxLogLength = config.TryGetValue("maxLogLength", out var mll) && int.TryParse(mll?.ToString(), out var mlli) ? mlli : 200;
            return extract;
        }

        private WaitForSelectorActivity CreateWaitForSelectorActivity(Dictionary<string, object> config, IServiceProvider serviceProvider)
        {
            var wait = ActivatorUtilities.CreateInstance<WaitForSelectorActivity>(serviceProvider);
            wait.Selector = config.TryGetValue("selector", out var ws) ? ws?.ToString() ?? string.Empty : string.Empty;
            wait.Timeout = config.TryGetValue("timeout", out var wt) && int.TryParse(wt?.ToString(), out var wti) ? wti : 30000;
            wait.State = config.TryGetValue("state", out var state) ? state?.ToString() ?? "visible" : "visible";
            return wait;
        }

        private ManualTriggerActivity CreateManualTriggerActivity(IServiceProvider serviceProvider)
        {
            return ActivatorUtilities.CreateInstance<ManualTriggerActivity>(serviceProvider);
        }
    }
} 