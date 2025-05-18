# Refactored Workflow Module Design: Article Summarization & Email

This document outlines the design for a refactored `Modules.Workflow` in ZenFlow, specifically tailored to implement the "Article Summarize and Sending to Email" scenario. This design addresses the issues identified in the current implementation and aligns with the robust JSON structure, Elsa, CQRS, and MassTransit principles previously discussed.

## 1. Workflow Definition (JSON Instance)

Below is an example of how the "Article Summarize and Sending to Email" workflow would be defined using the new JSON structure. This definition would be stored and managed by the ZenFlow backend.

```json
{
  "workflowId": "article-summarizer-v1",
  "name": "Daily Tech Article Summarizer & Emailer",
  "version": 1,
  "description": "Fetches the latest tech article from a news site, summarizes it using an AI service, and emails the summary.",
  "nodes": [
    {
      "id": "node_1_get_article_url",
      "type": "playwrightGetAttributeNode",
      "name": "Get Latest Article URL",
      "position": {"x": 100, "y": 100},
      "activityType": "ZenFlow.Activities.Playwright.GetElementAttributeActivity",
      "activityProperties": {
        "targetUrl": "{{ Variables.NewsSiteUrl }}/technology",
        "elementSelector": "div.latest-article-container > h2 > a",
        "attributeName": "href"
      },
      "executionStatus": {
        "currentState": "Pending",
        "lastUpdatedAt": "2025-05-14T00:00:00Z"
      },
      "backendIntegration": {
        "isAsync": false
      },
      "outputMappings": [
        {"sourceProperty": "attributeValue", "outputName": "LatestArticleUrl"}
      ]
    },
    {
      "id": "node_2_extract_content",
      "type": "playwrightExtractTextNode",
      "name": "Extract Article Content",
      "position": {"x": 100, "y": 250},
      "activityType": "ZenFlow.Activities.Playwright.ExtractTextFromElementActivity",
      "activityProperties": {
        "elementSelector": "div.article-body"
      },
      "executionStatus": {
        "currentState": "Pending",
        "lastUpdatedAt": "2025-05-14T00:00:00Z"
      },
      "backendIntegration": {
        "isAsync": false
      },
      "inputMappings": [
        {"targetProperty": "targetUrl", "sourceNodeId": "node_1_get_article_url", "sourceOutputName": "LatestArticleUrl"}
      ],
      "outputMappings": [
        {"sourceProperty": "extractedText", "outputName": "ArticleFullText"}
      ]
    },
    {
      "id": "node_3_summarize_article",
      "type": "aiSummarizeNode",
      "name": "Summarize Article (AI)",
      "position": {"x": 100, "y": 400},
      "activityType": "ZenFlow.Activities.AI.SummarizeTextActivity",
      "activityProperties": {
        "maxSummaryLength": 200
      },
      "executionStatus": {
        "currentState": "Pending",
        "lastUpdatedAt": "2025-05-14T00:00:00Z"
      },
      "backendIntegration": {
        "commandName": "SummarizeArticleCommand", // CQRS command
        "isAsync": true, // Assuming AI summarization can be long-running
        "eventToPublishOnSuccess": "ArticleSummarizationSucceededEvent",
        "eventToPublishOnFailure": "ArticleSummarizationFailedEvent"
      },
      "inputMappings": [
        {"targetProperty": "textToSummarize", "sourceNodeId": "node_2_extract_content", "sourceOutputName": "ArticleFullText"}
      ],
      "outputMappings": [
        {"sourceProperty": "summary", "outputName": "ArticleSummary"} // Populated from the success event
      ]
    },
    {
      "id": "node_4_send_email",
      "type": "sendEmailNode",
      "name": "Send Summary Email",
      "position": {"x": 100, "y": 550},
      "activityType": "ZenFlow.Activities.Communication.SendEmailActivity",
      "activityProperties": {
        "to": ["{{ Variables.RecipientEmail }}"],
        "subject": "Daily Tech News Summary: {{ Variables.ArticleTitle | default: \"Latest Article\" }}", // ArticleTitle could be another output from node_1 or node_2
        "isHtml": false
      },
      "executionStatus": {
        "currentState": "Pending",
        "lastUpdatedAt": "2025-05-14T00:00:00Z"
      },
      "backendIntegration": {
        "commandName": "SendWorkflowEmailCommand", // CQRS command
        "isAsync": true // Email sending can also be offloaded
      },
      "inputMappings": [
        {"targetProperty": "body", "sourceNodeId": "node_3_summarize_article", "sourceOutputName": "ArticleSummary"}
      ]
    }
  ],
  "edges": [
    {"id": "edge_1_to_2", "source": "node_1_get_article_url", "target": "node_2_extract_content"},
    {"id": "edge_2_to_3", "source": "node_2_extract_content", "target": "node_3_summarize_article"},
    {"id": "edge_3_to_4", "source": "node_3_summarize_article", "target": "node_4_send_email"}
  ],
  "globalVariables": {
    "NewsSiteUrl": "https://www.examplenews.com",
    "RecipientEmail": "user@example.com",
    "ArticleTitle": null
  },
  "metadata": {
    "createdAt": "2025-05-14T00:00:00Z",
    "updatedAt": "2025-05-14T00:00:00Z"
  }
}
```

## 2. Elsa Custom Activities Design

Each `activityType` in the JSON maps to a C# Elsa custom activity class. These activities will reside within the refactored `Modules.Workflow` (or a shared library if applicable).

*   **`ZenFlow.Activities.Playwright.GetElementAttributeActivity`**
    *   **Inputs**: `TargetUrl` (string), `ElementSelector` (string), `AttributeName` (string).
    *   **Logic**: Uses Playwright to navigate to `TargetUrl`, find the element specified by `ElementSelector`, and retrieve the value of `AttributeName`.
    *   **Outputs**: `AttributeValue` (string).
    *   **Backend Integration**: Synchronous. Directly executes Playwright logic.

*   **`ZenFlow.Activities.Playwright.ExtractTextFromElementActivity`**
    *   **Inputs**: `TargetUrl` (string), `ElementSelector` (string).
    *   **Logic**: Uses Playwright to navigate to `TargetUrl`, find the element by `ElementSelector`, and extract its inner text.
    *   **Outputs**: `ExtractedText` (string).
    *   **Backend Integration**: Synchronous.

*   **`ZenFlow.Activities.AI.SummarizeTextActivity`**
    *   **Inputs**: `TextToSummarize` (string), `MaxSummaryLength` (int, optional).
    *   **Logic**: This activity is a dispatcher. Based on `backendIntegration.commandName` (`SummarizeArticleCommand`), it constructs and sends this command via MediatR. If `isAsync` is true, it expects the command handler to publish a message to MassTransit (e.g., `InitiateSummarizationMessage`). The activity then suspends, waiting for a correlating `ArticleSummarizationSucceededEvent` or `ArticleSummarizationFailedEvent` via MassTransit to resume and provide the output.
    *   **Outputs**: `Summary` (string) - populated upon receiving the success event.
    *   **Backend Integration**: Asynchronous. Triggers `SummarizeArticleCommand`. Listens for `ArticleSummarizationSucceededEvent` / `ArticleSummarizationFailedEvent`.

*   **`ZenFlow.Activities.Communication.SendEmailActivity`**
    *   **Inputs**: `To` (List<string>), `Cc` (List<string>, optional), `Bcc` (List<string>, optional), `From` (string, optional - could come from system config), `Subject` (string), `Body` (string), `IsHtml` (bool).
    *   **Logic**: Based on `backendIntegration.commandName` (`SendWorkflowEmailCommand`), it constructs and sends this command. If `isAsync` is true, the command handler would publish a message to MassTransit (e.g., `SendEmailMessage`). The activity might complete immediately after dispatching if email sending is fire-and-forget for the workflow, or it could wait for a confirmation event if needed.
    *   **Outputs**: (Optional) `EmailSentStatus` (bool).
    *   **Backend Integration**: Asynchronous (recommended). Triggers `SendWorkflowEmailCommand`.

## 3. CQRS and MassTransit Integration

*   **Commands (CQRS)**:
    *   `SummarizeArticleCommand`: Handler contains logic to call an external AI service. If the call is long, it publishes an `InitiateSummarizationMessage` to MassTransit and returns. A separate MassTransit consumer handles the AI call and publishes success/failure events.
    *   `SendWorkflowEmailCommand`: Handler prepares and sends an email, possibly via a MassTransit message to an email sending service/consumer.
*   **MassTransit Consumers**:
    *   Consumer for `InitiateSummarizationMessage`: Calls the AI service, then publishes `ArticleSummarizationSucceededEvent` (with summary and correlation ID) or `ArticleSummarizationFailedEvent` (with error and correlation ID).
    *   Consumer for `SendEmailMessage` (if email sending is fully offloaded): Handles the actual email dispatch.
*   **Elsa Subscriptions (MassTransit)**:
    *   Elsa workflow engine will have triggers/activities that subscribe to `ArticleSummarizationSucceededEvent` and `ArticleSummarizationFailedEvent`. These will correlate the event back to the waiting `SummarizeTextActivity` instance using a correlation ID (e.g., `WorkflowInstanceId` + `NodeId`).

## 4. Data Flow

*   Data flows as defined by `inputMappings` and `outputMappings` in the node JSON.
*   Elsa's expression evaluator (e.g., Liquid) will be used to resolve values from `Variables` (global workflow variables) or outputs of previous nodes.
*   Example: `node_2_extract_content`'s `targetUrl` input is mapped from `node_1_get_article_url`'s `LatestArticleUrl` output.

## 5. Persistence

*   **Workflow Definitions**: The JSON workflow definitions will be stored in a database (e.g., PostgreSQL, SQL Server) likely in a table with columns for `WorkflowId`, `Name`, `Version`, and the `DefinitionJson` (storing the entire JSON string or broken down if preferred for querying specific top-level fields).
*   **Workflow Instances & Execution State**: When a workflow runs (`/api/workflows/{id}/run`):
    *   A `WorkflowRun` (or `WorkflowInstance`) entity is created.
    *   `NodeExecution` entities are created for each node as it executes. These will store the rich `executionStatus` (currentState, timestamps, error messages, logs, etc.) and `inputJson`/`outputJson` for auditing and debugging.
    *   The `executionStatus` object from the JSON design will directly map to persisted fields in the `NodeExecution` entity or a related table/JSONB column.

## 6. API Endpoints (Conceptual)

*   **`POST /api/workflows`**: Create a new workflow definition (accepts the workflow JSON).
*   **`PUT /api/workflows/{id}`**: Update an existing workflow definition.
*   **`GET /api/workflows/{id}`**: Retrieve a workflow definition.
*   **`GET /api/workflows`**: List workflow definitions.
*   **`POST /api/workflows/{id}/run`**: Execute a workflow. 
    *   Accepts: Workflow ID. Optionally, initial `globalVariables` for this run.
    *   Response: Workflow Run ID, initial status.
*   **`GET /api/workflow-runs/{runId}`**: Get the status of a specific workflow run, including the status of all its nodes.
*   **`GET /api/workflow-runs/{runId}/nodes/{nodeId}`**: Get detailed status and logs for a specific node in a specific run.

These endpoints would be implemented using Carter modules, with handlers dispatching CQRS commands/queries.

This refactored design provides a clear separation of concerns, leverages the detailed JSON structure for definition and status tracking, and integrates cleanly with Elsa, CQRS, and MassTransit for robust and scalable workflow execution.

