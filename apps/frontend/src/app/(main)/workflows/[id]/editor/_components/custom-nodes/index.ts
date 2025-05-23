import HttpRequestNode from "./http-request-node";
import SendEmailNode from "./send-email-node";
import ExtractDataNode from "./extract-data-node";
import TransformDataNode from "./transform-data-node";

// Export all node types as a single object for use in the flow editor
export const nodeTypes = {
    "ZenFlow.Activities.Http.HttpRequestActivity": HttpRequestNode,
    "ZenFlow.Activities.Email.SendEmailActivity": SendEmailNode,
    "ZenFlow.Activities.Playwright.ExtractTextFromElementActivity": ExtractDataNode,
    "ZenFlow.Activities.Data.TransformDataActivity": TransformDataNode,
};