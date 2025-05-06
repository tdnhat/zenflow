import { ManualTriggerNode } from "./manual-trigger-node";
import { NavigateNode } from "./navigate-node";
import { InputTextNode } from "./input-text-node";
import { WaitForSelectorNode } from "./wait-for-selector-node";
import { ScreenshotNode } from "./screenshot-node";
import { CrawlDataNode } from "./crawl-data-node";
import { ClickNode } from "./click-node";

// Export all node types as a single object for use in the flow editor
export const nodeTypes = {
    "manual-trigger": ManualTriggerNode,
    navigate: NavigateNode,
    click: ClickNode,
    "input-text": InputTextNode,
    "wait-for-selector": WaitForSelectorNode,
    screenshot: ScreenshotNode,
    "crawl-data": CrawlDataNode,
};

// Export individual components for direct use
export {
    ManualTriggerNode,
    NavigateNode,
    InputTextNode,
    WaitForSelectorNode,
    ScreenshotNode,
    CrawlDataNode,
    ClickNode
}; 