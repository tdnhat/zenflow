# ZenFlow API Endpoints: Workflow Execution & Management

This document defines the main API endpoints for managing and executing workflows within the refactored ZenFlow system. These endpoints are designed to be RESTful and align with the CQRS pattern, Carter for routing, and the previously defined JSON structures for workflow definitions and status reporting.

## 1. Workflow Definition Management

These endpoints handle the CRUD operations for workflow definitions.

### 1.1. Create Workflow

*   **Endpoint**: `POST /api/workflows`
*   **Description**: Creates a new workflow definition.
*   **Request Body**: The complete workflow JSON definition (as per `core_workflow_definition.md` and the example in `refactored_workflow_module_design.md`).
    ```json
    // Example: (Full workflow JSON as previously defined)
    {
      "workflowId": "user-defined-id-or-null-for-generated", // Optional, backend can generate if null
      "name": "My New Workflow",
      "version": 1,
      "description": "A description of my new workflow.",
      "nodes": [ /* ... */ ],
      "edges": [ /* ... */ ],
      "globalVariables": { /* ... */ },
      "metadata": { /* ... */ }
    }
    ```
*   **Response (Success - 201 Created)**:
    *   Body: The created workflow definition JSON, including any backend-generated fields (e.g., `workflowId` if not provided, `createdAt`, `updatedAt` in metadata).
    *   Headers: `Location: /api/workflows/{new_workflow_id}`
*   **Response (Error - 400 Bad Request)**: If the request body is invalid (e.g., malformed JSON, missing required fields, validation errors against the schema).
    ```json
    {
      "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
      "title": "One or more validation errors occurred.",
      "status": 400,
      "errors": {
        "name": ["The Name field is required."],
        "nodes[0].activityType": ["Unsupported activity type."]
      }
    }
    ```
*   **Backend Logic**: Dispatches a `CreateWorkflowCommand` (CQRS). The handler validates the definition, persists it, and returns the created entity.

### 1.2. Get Workflow Definition

*   **Endpoint**: `GET /api/workflows/{workflowId}`
*   **Description**: Retrieves a specific workflow definition by its ID.
*   **Path Parameters**:
    *   `workflowId` (string, required): The unique identifier of the workflow.
*   **Response (Success - 200 OK)**:
    *   Body: The workflow definition JSON.
*   **Response (Error - 404 Not Found)**: If no workflow with the given ID exists.
*   **Backend Logic**: Dispatches a `GetWorkflowDefinitionQuery` (CQRS).

### 1.3. Update Workflow Definition

*   **Endpoint**: `PUT /api/workflows/{workflowId}`
*   **Description**: Updates an existing workflow definition. This typically creates a new version if versioning is implemented, or updates the existing one.
*   **Path Parameters**:
    *   `workflowId` (string, required): The ID of the workflow to update.
*   **Request Body**: The complete updated workflow JSON definition.
*   **Response (Success - 200 OK)**:
    *   Body: The updated workflow definition JSON.
*   **Response (Error - 400 Bad Request)**: Invalid request body.
*   **Response (Error - 404 Not Found)**: Workflow ID does not exist.
*   **Backend Logic**: Dispatches an `UpdateWorkflowCommand` (CQRS).

### 1.4. List Workflow Definitions

*   **Endpoint**: `GET /api/workflows`
*   **Description**: Retrieves a list of all workflow definitions. Supports pagination.
*   **Query Parameters**:
    *   `page` (int, optional, default: 1): Page number.
    *   `pageSize` (int, optional, default: 20): Number of items per page.
    *   `searchTerm` (string, optional): Filter workflows by name or description.
*   **Response (Success - 200 OK)**:
    ```json
    {
      "page": 1,
      "pageSize": 20,
      "totalPages": 5,
      "totalCount": 98,
      "items": [
        // Array of workflow definition JSON objects (summary or full)
      ]
    }
    ```
*   **Backend Logic**: Dispatches a `ListWorkflowDefinitionsQuery` (CQRS).

### 1.5. Delete Workflow Definition

*   **Endpoint**: `DELETE /api/workflows/{workflowId}`
*   **Description**: Deletes a workflow definition. Consider implications for running instances or historical data (soft delete vs. hard delete).
*   **Path Parameters**:
    *   `workflowId` (string, required): The ID of the workflow to delete.
*   **Response (Success - 204 No Content)**.
*   **Response (Error - 404 Not Found)**: Workflow ID does not exist.
*   **Backend Logic**: Dispatches a `DeleteWorkflowCommand` (CQRS).

## 2. Workflow Execution and Run Management

These endpoints handle the initiation and monitoring of workflow runs.

### 2.1. Run Workflow

*   **Endpoint**: `POST /api/workflows/{workflowId}/run`
*   **Description**: Initiates a new execution (run) of a specified workflow definition.
*   **Path Parameters**:
    *   `workflowId` (string, required): The ID of the workflow definition to run.
*   **Request Body (Optional)**:
    ```json
    {
      "globalVariables": {
        "InitialInputParameter": "someValue",
        "AnotherParameter": 123
      },
      "correlationId": "user-provided-correlation-id" // Optional, for client-side tracking
    }
    ```
*   **Response (Success - 202 Accepted)**: Indicates the request to run the workflow has been accepted. The actual execution is likely asynchronous.
    ```json
    {
      "workflowRunId": "generated-run-id-123",
      "workflowId": "article-summarizer-v1",
      "status": "Pending", // Or "Starting"
      "requestedAt": "2025-05-14T06:30:00Z",
      "message": "Workflow run initiated."
    }
    ```
*   **Response (Error - 404 Not Found)**: If the `workflowId` does not exist.
*   **Response (Error - 400 Bad Request)**: If initial variables are invalid or other pre-run checks fail.
*   **Backend Logic**: 
    1.  Dispatches a `RequestWorkflowRunCommand` (CQRS).
    2.  The handler validates the request, creates a `WorkflowRun` entity with a `Pending` status.
    3.  It then likely publishes a message (e.g., `StartWorkflowExecutionMessage`) to MassTransit, which an Elsa-aware consumer picks up to start the actual Elsa workflow instance.

### 2.2. Get Workflow Run Status

*   **Endpoint**: `GET /api/workflow-runs/{workflowRunId}`
*   **Description**: Retrieves the current status and details of a specific workflow run, including the status of its individual nodes.
*   **Path Parameters**:
    *   `workflowRunId` (string, required): The unique identifier of the workflow run.
*   **Response (Success - 200 OK)**:
    ```json
    {
      "workflowRunId": "generated-run-id-123",
      "workflowId": "article-summarizer-v1",
      "status": "Running", // e.g., Pending, Running, Completed, Failed, Cancelled
      "startedAt": "2025-05-14T06:30:05Z",
      "completedAt": null,
      "globalVariablesSnapshot": { /* ... */ },
      "nodes": [
        // Array of node execution status objects (as per the `executionStatus` in the main JSON design)
        {
          "nodeId": "node_1_get_article_url",
          "name": "Get Latest Article URL",
          "activityType": "ZenFlow.Activities.Playwright.GetElementAttributeActivity",
          "currentState": "Completed",
          "startedAt": "2025-05-14T06:30:10Z",
          "completedAt": "2025-05-14T06:30:15Z",
          "durationMs": 5000,
          "inputJson": "{...}", // Sanitized/relevant inputs
          "outputJson": "{\"attributeValue\": \"https://...\"}", // Sanitized/relevant outputs
          "errorDetails": null,
          "logs": ["Node started.", "Playwright action successful.", "Node completed."]
        },
        {
          "nodeId": "node_3_summarize_article",
          "name": "Summarize Article (AI)",
          "currentState": "Running", // Or "WaitingForEvent"
          "startedAt": "2025-05-14T06:30:20Z",
          // ... other status fields
        }
      ]
    }
    ```
*   **Response (Error - 404 Not Found)**: If the `workflowRunId` does not exist.
*   **Backend Logic**: Dispatches a `GetWorkflowRunStatusQuery` (CQRS). The handler retrieves the `WorkflowRun` and its associated `NodeExecution` records, then formats them according to the defined response structure.

### 2.3. List Workflow Runs

*   **Endpoint**: `GET /api/workflow-runs`
*   **Description**: Retrieves a list of workflow runs, filterable and paginated.
*   **Query Parameters**:
    *   `workflowId` (string, optional): Filter by a specific workflow definition ID.
    *   `status` (string, optional): Filter by status (e.g., "Running", "Completed", "Failed").
    *   `page` (int, optional, default: 1).
    *   `pageSize` (int, optional, default: 20).
    *   `sortBy` (string, optional, e.g., "startedAt_desc").
*   **Response (Success - 200 OK)**: Paginated list of workflow run summary objects.
    ```json
    {
      "page": 1,
      "pageSize": 20,
      "totalPages": 3,
      "totalCount": 55,
      "items": [
        {
          "workflowRunId": "run-id-1",
          "workflowId": "article-summarizer-v1",
          "status": "Completed",
          "startedAt": "2025-05-13T10:00:00Z",
          "completedAt": "2025-05-13T10:05:00Z"
        }
        // ... other run summaries
      ]
    }
    ```
*   **Backend Logic**: Dispatches a `ListWorkflowRunsQuery` (CQRS).

### 2.4. Cancel Workflow Run (Optional)

*   **Endpoint**: `POST /api/workflow-runs/{workflowRunId}/cancel`
*   **Description**: Attempts to cancel an ongoing workflow run.
*   **Path Parameters**:
    *   `workflowRunId` (string, required).
*   **Response (Success - 202 Accepted)**: Request to cancel has been accepted.
    ```json
    {
      "workflowRunId": "generated-run-id-123",
      "status": "Cancelling",
      "message": "Workflow cancellation requested."
    }
    ```
*   **Response (Error - 404 Not Found)**.
*   **Response (Error - 409 Conflict)**: If the workflow is already completed or cannot be cancelled.
*   **Backend Logic**: Dispatches a `CancelWorkflowRunCommand`. This might involve Elsa sending a cancellation signal to the running workflow instance or publishing a specific event via MassTransit that Elsa activities are listening for.

## General Considerations

*   **Authentication & Authorization**: All endpoints must be secured. Authorization rules might apply (e.g., only owners can delete/update workflows, specific roles can run workflows).
*   **Error Handling**: Consistent error response format (e.g., RFC 7807 Problem Details).
*   **Versioning**: API versioning (e.g., `/api/v1/workflows`) should be considered for future evolution.
*   **Idempotency**: For `POST` operations that create resources, consider mechanisms to handle retries and prevent duplicate resource creation if clients retry (e.g., using an `Idempotency-Key` header).

This API design provides a comprehensive set of endpoints for managing workflow definitions and their executions, aligning with the architectural goals of the ZenFlow project.

