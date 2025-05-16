# ZenFlow Integration Outline: Workflow, Elsa, CQRS, MassTransit & JSON

This document outlines the integration points between the refactored ZenFlow `Modules.Workflow`, the Elsa workflow engine, the CQRS pattern, MassTransit for messaging, and the defined JSON structure. This integration is designed to create a robust, modular, and scalable workflow automation system.

## 1. Introduction

The refactored ZenFlow system aims to provide a flexible platform for defining, executing, and monitoring complex workflows. The core components involved are:

*   **JSON Workflow Structure**: The declarative definition of a workflow, its nodes, edges, and configurations.
*   **Elsa Workflow Engine**: The runtime that executes workflow definitions.
*   **CQRS (Command Query Responsibility Segregation)**: An architectural pattern for separating read and write operations, typically using MediatR for in-process messaging.
*   **MassTransit**: A distributed application framework for .NET, used here for asynchronous messaging and event-driven communication.
*   **Carter**: Minimalist routing for API endpoints.

## 2. JSON Structure: The Core Contract

The JSON structure (detailed in `core_workflow_definition.md`, `node_definition.md`, etc.) is the central contract that binds the system together:

*   **Frontend (React Flow)**: The Next.js React Flow editor creates and visualizes this JSON structure, allowing users to design workflows graphically.
*   **Backend Storage**: The backend stores these JSON definitions (e.g., in a `WorkflowDefinitions` table). The `DefinitionJson` can be stored as a whole or partially de-structured for querying key metadata.
*   **Elsa Activity Configuration**: The `nodes[].activityType` and `nodes[].activityProperties` within the JSON directly inform Elsa which custom activity to execute and how to configure it.
*   **Execution and Status Tracking**: The `nodes[].executionStatus` sub-object within the JSON definition provides the model for how status is tracked and reported back via the API. The `nodes[].backendIntegration` section dictates how a node interacts with CQRS/MassTransit.

## 3. Elsa Workflow Engine Integration

Elsa is the heart of the workflow execution.

*   **Workflow Definition Registration/Instantiation**: 
    *   When a workflow run is initiated (e.g., via `POST /api/workflows/{id}/run`), the backend retrieves the stored JSON definition.
    *   This JSON definition is then used to dynamically construct or provide to an Elsa `IWorkflowBuilder` or similar mechanism to create a `WorkflowBlueprint`. Elsa can then instantiate and run this blueprint.
    *   Alternatively, if Elsa 3.0+ offers direct JSON-based workflow definition import that aligns with our structure, that can be leveraged.
*   **Custom Elsa Activities**: 
    *   Each `activityType` string in the node JSON (e.g., `"ZenFlow.Activities.Playwright.GetElementAttributeActivity"`) maps to a specific C# class inheriting from Elsa's `Activity` or `CodeActivity`.
    *   These custom activities are designed to be configurable. Their input properties (e.g., `TargetUrl`, `ElementSelector`) are populated by Elsa from the `activityProperties` object in the corresponding JSON node definition.
    *   Elsa manages the lifecycle of these activities during workflow execution.
*   **Elsa Execution Lifecycle**: 
    *   Elsa traverses the workflow graph based on the `nodes` and `edges` defined in the JSON.
    *   It manages the state of the workflow instance (e.g., `WorkflowInstance` in Elsa terms) and the state of individual activities.
    *   Persistence of workflow instance state is handled by Elsa, configured with a persistence provider (e.g., EF Core pointing to the ZenFlow database).
*   **Input/Output Handling & Data Flow**: 
    *   Elsa activities receive inputs as defined by their C# properties. The values for these inputs are resolved by Elsa based on the `inputMappings` in the JSON node definition. This mapping can refer to workflow variables or outputs of previous nodes.
    *   `outputMappings` in the JSON node definition declare how the outputs of an Elsa activity are captured and named, making them available for subsequent nodes or for recording in `NodeExecution.OutputJson`.
    *   Elsa's expression evaluators (e.g., Liquid, JavaScript) are used to evaluate expressions within mappings or properties (e.g., `"{{ Variables.NewsSiteUrl }}"`).

## 4. CQRS Integration

CQRS helps organize the application logic, especially for API interactions and operations triggered by workflows.

*   **API Endpoints to Commands/Queries**: 
    *   Carter API modules (defined in `zenflow_api_endpoints.md`) receive HTTP requests.
    *   Handlers within these modules translate requests into CQRS commands (for write operations like creating a workflow, starting a run) or queries (for read operations like fetching a workflow definition or run status).
    *   These commands/queries are dispatched using a mediator library (e.g., MediatR).
    *   Example: `POST /api/workflows/{id}/run` dispatches a `RequestWorkflowRunCommand`.
*   **Elsa Activities to Commands**: 
    *   Certain custom Elsa activities can act as dispatchers for CQRS commands. The `backendIntegration.commandName` property in the JSON node definition specifies the command to be dispatched.
    *   Example: `SummarizeTextActivity` might dispatch a `SummarizeArticleCommand` via MediatR from within its `ExecuteAsync` method.
*   **Command/Query Handlers**: 
    *   These handlers contain the core business logic.
    *   They interact with domain entities, repositories (for database access), and other services.
    *   Command handlers are responsible for persisting state changes and can publish domain events (often via MassTransit for broader consumption).

## 5. MassTransit Integration

MassTransit facilitates asynchronous operations, event-driven architectures, and decoupling of services.

*   **Asynchronous Operations in Elsa Activities**: 
    *   When an Elsa activity needs to perform a long-running task (e.g., calling an external AI service), it can use MassTransit.
    *   The activity (or a CQRS command handler it calls) publishes a message to a MassTransit queue (e.g., `InitiateSummarizationMessage`).
    *   The Elsa activity can then suspend itself (e.g., by returning a `Suspend()` outcome or using Elsa's native bookmarking for external events).
*   **Event Publishing & Consumption**: 
    *   **From CQRS/Consumers**: After an asynchronous operation completes, the MassTransit consumer (or CQRS handler) publishes a result event (e.g., `ArticleSummarizationSucceededEvent`, `ArticleSummarizationFailedEvent`). These events contain correlation IDs.
    *   **Elsa Subscriptions**: Elsa workflows can be designed with activities that are triggered by specific MassTransit messages (events). Elsa's runtime subscribes to these messages. When an event is received, Elsa uses the correlation ID to find the correct suspended workflow instance and resume it, passing the event data to the waiting activity.
    *   The `backendIntegration.eventToPublishOnSuccess` and `eventToPublishOnFailure` fields in the node JSON can inform which events an activity might produce or expect.
*   **Outbox Pattern**: 
    *   To ensure reliable event publishing (especially after database transactions commit), the outbox pattern (e.g., using `WorkflowOutboxMessage.cs`) is employed. Events are first written to an outbox table in the same transaction as domain changes. A separate process then relays these messages to MassTransit.
*   **Correlation**: 
    *   Crucial for asynchronous operations. Messages and events in MassTransit must carry correlation IDs (e.g., `WorkflowRunId`, `NodeExecutionId`, or a specific `CorrelationId` generated for the operation). This allows Elsa to route incoming events back to the correct, waiting workflow instance and activity.

## 6. Data Flow and State Management Summary

1.  **Definition**: User designs workflow in React Flow, producing the **JSON structure**.
2.  **Storage**: API (via CQRS command) stores this JSON in the database.
3.  **Initiation**: User triggers `/run` API endpoint. A CQRS command creates a `WorkflowRun` record and publishes a message to MassTransit to start **Elsa** execution.
4.  **Execution**: Elsa instantiates the workflow from the JSON. Custom Elsa activities execute, configured by `activityProperties`.
    *   Synchronous activities execute directly.
    *   Asynchronous activities may dispatch CQRS commands or publish **MassTransit** messages and suspend.
5.  **Async Handling**: MassTransit consumers process messages, perform work, and publish result events (with correlation IDs) back to MassTransit.
6.  **Resumption**: Elsa, listening to these events, resumes the correct workflow instance.
7.  **Status Updates**: Throughout execution, Elsa activities (or related services) update `NodeExecution` records in the database with status, inputs, outputs, and logs, reflecting the `executionStatus` model from the JSON.
8.  **Monitoring**: API (via CQRS queries) reads `WorkflowRun` and `NodeExecution` data to provide status updates to the user.

## 7. Modularity and Scalability

*   **Modularity**: 
    *   Custom Elsa activities are self-contained units of work.
    *   CQRS promotes separation of read/write logic and feature-based vertical slices.
    *   MassTransit decouples message producers from consumers.
*   **Scalability**: 
    *   Asynchronous processing via MassTransit allows long-running tasks to be handled by dedicated consumers, which can be scaled independently.
    *   Elsa's design, especially with durable persistence, supports scalable workflow execution.
    *   Statelessness of API endpoints and, where possible, Elsa activities, aids scalability.

This integrated architecture provides a powerful and flexible foundation for ZenFlow's workflow automation capabilities, ensuring clear contracts, separation of concerns, and robust execution.

