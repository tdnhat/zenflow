// Helper functions and constants for workflow nodes

// Map of backend activity types to their display names
export const nodeTypeDisplayNames: Record<string, string> = {
    "ZenFlow.Activities.Http.HttpRequestActivity": "HTTP Request",
    "ZenFlow.Activities.Email.SendEmailActivity": "Send Email",
    "ZenFlow.Activities.Playwright.ExtractTextFromElementActivity": "Extract Data",
    "ZenFlow.Activities.Data.TransformDataActivity": "Transform Data",
    // Add other mappings as needed
};

// Finds the display title for a given node type
export const findNodeTitle = (
    nodeType: string,
    displayNames: Record<string, string> = nodeTypeDisplayNames
): string => {
    return displayNames[nodeType] || nodeType;
}; 