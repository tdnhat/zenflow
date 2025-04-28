# Software Requirements Specification (SRS): ZenFlow

## 1. Introduction

**ZenFlow** is a modern web platform for managing organizational workflows and tasks with an advanced visual workflow editor. It will enable users to create, assign, and track tasks ("flows") in a flexible, secure way through an intuitive, drag-and-drop visual interface similar to n8n. The system will be built as a **full-stack application**: a **Next.js** (React) frontend and a **.NET 8** backend. This SRS outlines the system's scope, architecture, and requirements, emphasizing modular, scalable design. The frontend will leverage modern React libraries including **React Flow** for the visual workflow editor, **Chakra UI** for components, **Zustand** for state management, **TanStack React Query** for data fetching, **Axios** for API calls, **React Hook Form** for form handling, and **React Hot Toast** for notifications. The frontend and backend will live in a **monorepo**, with shared libraries for common code (e.g. types, models). The design adopts a **modular monolith** backend architecture (a "Modulith") -- i.e., a single deployed application divided into independent modules -- which "**takes the best of both worlds**" of monoliths and microservices. Each module will use **Vertical Slice Architecture** (feature folders) with Domain-Driven Design and CQRS via MediatR to isolate functionality by use case. Authentication and authorization will use **OAuth2/OpenID Connect** via Keycloak, providing single sign-on, user management and fine-grained roles. Message-based, asynchronous communication will use RabbitMQ with MassTransit, implementing the **Transactional Outbox Pattern** to ensure reliability. PostgreSQL (with EF Core code-first) will store data, and Redis will act as a distributed cache to improve performance.

## 2. System Overview

ZenFlow consists of two main components: the **Frontend** (Next.js app) and the **Backend** (ASP.NET Core Modulith). They share code in a monorepo. The frontend will be structured around **Next.js App Router** (or Pages Router) with separate folders for features, components, hooks, contexts, and services. For example, a typical structure might be:

```
/src
  /app or /pages # Next.js routes
  /components    
    /ui          # Reusable UI components (buttons, forms, etc.)
    /features    # Feature-specific components (WorkflowEditor, NodePanel)
    /layouts     # Layout components (Sidebar, Header, etc.)
  /contexts      # React Context providers (e.g. AuthContext, ThemeContext)
  /hooks         # Custom hooks (e.g. useAuth, useApi, useWorkflow)
  /services      # API client code, business logic utilities
  /lib           # Technical infrastructure (axios setup, query client)
  /store         # Zustand stores (workflowStore, authStore, etc.)
  /styles        # Global and component styles
  /utils         # Utility functions
  /public        # Public assets
```

A core principle is "separation of concerns": UI components live under components/, application logic like API calls or auth helpers go into a services/ folder, and global state management uses Zustand stores in store/. Utility functions (pure helpers) belong in utils/. The frontend will incorporate several key technologies to deliver a seamless workflow automation experience:

1. **React Flow**: Provides the core drag-and-drop graph editor for building visual workflows. It enables users to create nodes, connect them with edges, and manage the flow of data/actions visually. React Flow will be customized with ZenFlow-specific node types, edge styles, and controls.

2. **Zustand**: A lightweight state management solution that replaces the need for Redux or Context API for managing global state. Zustand will manage the workflow editor state, authentication state, and theme preferences.

3. **Chakra UI**: A component library with accessibility baked in, providing a consistent design system for all UI elements. Chakra UI components will be used throughout the application for forms, buttons, modals, and other UI elements.

4. **TanStack React Query**: Handles data fetching, caching, and state synchronization with the server. It will manage all API interactions for workflow data, user data, and system configuration.

5. **Axios**: Used for making HTTP requests to the backend API. Will be configured with interceptors for handling authentication tokens and error responses.

6. **React Hook Form**: Manages form state and validation for all forms in the application, particularly for node configuration panels in the workflow editor.

7. **React Hot Toast**: Provides a notification system for user feedback on actions (success, errors, warnings).

The backend is a **Modular Monolith**. Conceptually, ZenFlow is one application, but code is partitioned into **modules** (bounded contexts) by feature. Each module contains its own domain models, data access, and API endpoints. For example, there might be modules like UserManagement, Workflow, and Notifications. Each module is implemented with vertical slices: features (use cases) are grouped in folders containing all layers (API endpoint, input validation, handlers, domain logic). This means instead of traditional layered code, each HTTP request maps to a "slice" of code. Modules communicate with each other via asynchronous messaging when needed (e.g. publishing a WorkflowCreated event), but within the same process they may also call each other's APIs or services if appropriate. Importantly, the architecture allows eventual extraction of modules into separate services ("microservices") in the future, since modules are loosely coupled and rely only on well-defined APIs.

Key architectural elements:

- **.NET 8 and Minimal APIs**: We will use ASP.NET Core Minimal APIs to define HTTP endpoints with minimal boilerplate. This keeps controllers lightweight and focuses on endpoints for each feature.

- **Domain-Driven Design + CQRS**: Each module maintains a domain model. Commands/queries are handled via MediatR, implementing CQRS; this encourages clear separation of write/read logic and easy unit testing.

- **Entity Framework Core (Code-First) & PostgreSQL**: Each module can have its own DbContext or share one, with EF Core migrations for schema management. PostgreSQL is the relational database.

- **Outbox Pattern and Messaging**: To ensure reliable, transactional message publishing, we will use MassTransit's EF Core outbox. This writes outgoing messages to a database table in the same transaction as business operations. A background service then delivers these messages to RabbitMQ. RabbitMQ (via MassTransit) provides publish/subscribe and request/response messaging between modules. As MassTransit documentation notes, even a monolith "can be [a] messaging powerhouse" -- messaging "simplifies architecture and enhances scalability" by decoupling components.

- **Authentication/Authorization**: All user auth is centralized through Keycloak (OAuth2/OIDC). ZenFlow trusts Keycloak tokens for user identity and roles. Keycloak offers SSO and robust IAM features out-of-the-box, so the application itself "doesn't have to deal with login forms" or user storage.

- **Shared Code / Monorepo**: Since this is a monorepo, shared libraries (e.g. TypeScript types for API models, utility functions, or a component library) are factored into packages that can be used by both frontend and backend. For example, common DTO definitions or validation schemas could live in a shared folder or package.

An overall **system context diagram** (conceptual) shows:

- Frontend (Next.js) ←→ Backend API (ASP.NET Minimal APIs)
- Backend modules communicate via RabbitMQ (MassTransit)
- Backend ←→ PostgreSQL (via EF Core)
- Backend ←→ Redis (distributed cache layer for read-heavy data)
- All services integrate with Keycloak for auth (OpenID Connect).

*(Note: While specific diagram images are not shown here, a typical architecture diagram would depict the single monolithic deployment with internal modules, databases, cache, and message broker as described above.)*

## 3. Functional Requirements

ZenFlow's functionality is organized into user-visible features and internal behaviors. Key functional requirements include:

- **User Authentication/Authorization (FR1)**: The system shall allow users to log in via OAuth2/OpenID Connect (Keycloak). Users must authenticate before accessing any protected functionality. The system shall support role-based access (e.g. admin vs regular user) as defined in Keycloak. Single sign-on (SSO) means a user logged in once to Keycloak can use ZenFlow without re-entering credentials.

- **User Management (FR2)**: Admin users shall be able to create, read, update, and delete user accounts. User data (profiles, roles) is managed through Keycloak's admin API or via integration. On user creation, the system may send a notification or event to other modules (e.g. welcome email).

- **Visual Workflow Editor (FR3)**: ZenFlow's core feature is a powerful, visual workflow editor similar to n8n, built with **React Flow** for the graph/editor interface. Users shall be able to:
  * Create workflows by dragging and dropping nodes onto a canvas
  * Connect nodes with edges to define the flow of data/actions
  * Configure nodes through property panels built with Chakra UI and React Hook Form
  * Save, load, and version workflows
  * Execute workflows manually or based on triggers (time-based, event-based, etc.)
  * View workflow execution history and logs
  * Create custom nodes for specific business processes
  * Import/export workflow definitions

- **Workflow Definition and Execution (FR4)**: The backend shall provide a comprehensive API for workflow definition storage and execution. The workflow engine will support:
  * Node type registration and discovery
  * Workflow execution (synchronous and asynchronous)
  * Workflow state management (pending, running, completed, failed)
  * Error handling and retry logic
  * Execution logs and history

- **Asynchronous Notifications (FR5)**: When certain events occur (e.g. a workflow is created or completes execution), the system shall publish a domain event message. For example, creating a workflow publishes a WorkflowCreated event to RabbitMQ. Other modules (or future services) may subscribe to these events. Event publishing uses the Outbox pattern so that, e.g., creating a workflow in the database and enqueuing the message occur in one transaction.

- **Data Consistency and Reliability (FR6)**: The system shall use **PostgreSQL** with EF Core for persistent storage of all domain data. Operations that affect multiple tables or modules must be handled transactionally within the monolith or via eventual consistency with messaging. The Outbox ensures message delivery even if the message broker or network is temporarily unavailable.

- **Caching (FR7)**: To improve read performance, ZenFlow shall cache frequently accessed data in **Redis**. For instance, user session information, workflow definitions, or node type metadata may be cached. The cache will be updated on data changes to ensure consistency.

- **Clean API Design (FR8)**: Backend APIs shall follow REST principles (or minimal API patterns) and use consistent request/response models. Data validation will be applied (e.g., model binding and FluentValidation or similar) so that invalid inputs are rejected.

- **Frontend Architecture (FR9)**: The Next.js frontend shall follow modern React patterns:
  * **Zustand** for global state management (workflow editor state, authentication, theme)
  * **TanStack React Query** for data fetching, caching, and synchronization with server state
  * **Axios** for API communication with interceptors for authentication and error handling
  * **React Hook Form** for all form handling with validation
  * **React Hot Toast** for user notifications/feedback
  * **Chakra UI** for consistent, accessible, and themeable UI components

- **Reusable Shared Code (FR10)**: Common code (e.g. TypeScript interfaces for API data, form components, validation schemas) will be placed in shared folders/packages to avoid duplication. For example, if the backend and frontend both use a WorkflowNodeDto definition, it can live in a shared libs/shared package in the monorepo.

- **Module Isolation (FR11)**: Internal backend modules shall be isolated. A module may expose internal services or a public API. Other modules should not directly access a module's database; all cross-module actions happen via messages or public APIs. This encapsulation allows each module to evolve independently.

## 4. Non-Functional Requirements

- **Performance & Scalability**: ZenFlow must perform efficiently under expected load. Using a modular design with asynchronous messaging helps horizontal scalability later. Caching with Redis will reduce database load for read-heavy operations. The application will run on ASP.NET Core which can handle high throughput. According to messaging literature, even a monolith "can be [a] messaging powerhouse"; applying messaging can "simplify architecture and enhance its scalability". In practice, background tasks (email sending, reports) will run asynchronously to keep APIs responsive.

- **Reliability**: Data integrity is paramount. The Outbox pattern and database transactions ensure that if a transaction fails, no partial state or unsent events remain. RabbitMQ guarantees at-least-once delivery; the system should be designed to handle duplicate events idempotently. Redis cache will be configured for persistence or failure recovery to avoid data loss.

- **Security**: All communication uses HTTPS. Backend APIs require valid JWT access tokens from Keycloak (OIDC) for authentication. Role-based access control (RBAC) rules defined in Keycloak determine access. Keycloak itself provides "strong authentication" and fine-grained authorization, which the application enforces. Sensitive data (passwords, tokens) will not be stored in the frontend. Proper CORS and CSP policies will be set.

- **Maintainability and Extensibility**: The codebase must be clean, well-organized, and follow SOLID principles. Vertical-slice organization minimizes cross-cutting dependencies, making it easier to modify one feature without affecting others. Documentation should cover module boundaries. As modules are loosely coupled, teams can extend functionality by adding new modules or extracting existing ones. The monorepo and shared libraries encourage code reuse. Unit and integration tests will be written for each module.

- **Usability**: The frontend will be intuitive. The structure (contexts, hooks, services) should allow junior developers to quickly find where to add features. UI design guidelines (such as component library or design tokens) will be applied for consistency.

- **Deployability**: The application should be containerizable for deployment. Using a monorepo with Terraform or Docker Compose, we can manage services (API, DB, RabbitMQ, Redis, Keycloak). The architecture is a "Modulith" -- one deployable -- simplifying CI/CD initially.

## 5. System Architecture & Design

### 5.1 Overall Architecture

The system is a **Modular Monolith (Modulith)**. All backend modules run in a single process but are logically separated. Each module may have its own EF Core context or share contexts; modules communicate via messages or HTTP API. This approach "combines the benefits of modular design with the simplicity of a monolithic architecture". In a modular monolith, the application is built and deployed as one unit, but **code** is organized into well-defined, loosely-coupled modules. For example, a Workflow module contains everything needed to manage workflows: its database tables, domain model, service classes, API handlers, and background consumers. Likewise, a User module manages user profiles, roles, and Keycloak integration (syncing data or reacting to user events).

MassTransit will mediate communication. Typical patterns:

- **In-Process Calls**: If one module needs a quick query from another and low latency is needed, it could call an internal service or Mediator in the same process (since everything is one app, we can inject other module services).

- **Messaging (Async)**: For decoupled updates or notifications, modules publish events. E.g., the Workflow module might publish WorkflowCompleted to RabbitMQ. Other modules (like Notification) subscribe and act (send emails). This decoupling aids maintainability.

Internally, **MediatR** is used for implementing requests within a module: e.g., sending a CreateWorkflowCommand or GetWorkflowQuery through MediatR handlers. This enforces the vertical slice: each request has its own request/handler pair, often in a folder by feature name. As Jimmy Bogard explains, this "architecture is built around distinct requests, encapsulating and grouping all concerns from front-end to back". Dependencies between slices are minimized, so each feature can be developed or tested in isolation.

**Data Layer**

- **Entity Framework Core**: Code-first approach. Domain entities and mappings reside in module-specific libraries or folders. Migrations can be organized per module or globally. EF ensures data consistency.

- **PostgreSQL**: A single Postgres instance holds all module tables (or separate schemas). Because modules are in one app, cross-DB transactions are easier. If needed, we could spin up multiple databases/schemas per module, but initially a single database with schema separation should suffice.

- **Redis Cache**: A Redis instance will act as a distributed cache. For example, after a user logs in, their user profile could be cached to avoid repeated database lookups. Cache invalidation is managed in write operations.

**Infrastructure and Messaging**

- **MassTransit + RabbitMQ**: All modules share a RabbitMQ broker via MassTransit. We'll configure Exchange/Queue names per event and use publish/subscribe or send/receive as needed.

- **Transactional Outbox**: MassTransit's EF Core outbox support will be enabled (via AddEntityFrameworkOutbox) so that any messages published in a handler are first saved to the database tables OutboxMessage/OutboxState. A background *delivery service* (included in MassTransit) will read from these tables and send messages to RabbitMQ. This guarantees that if a transaction commits but the broker is down, the message will be sent later, achieving at-least-once delivery. The outbox tables include an inbox tracking to avoid duplicate consumer deliveries.

- **Domain Event Flow**: When a module performs an operation (e.g. WorkflowCreated), it raises a domain event handled in a Handler that publishes a message via MassTransit. The outbox ensures atomicity. Other modules subscribe and act.

**Security**

- **Keycloak (OAuth2/OpenID)**: Keycloak runs as a separate service. On the backend, ASP.NET will use AddAuthentication().AddJwtBearer() pointed to Keycloak's public key for token validation. All API endpoints require authentication unless explicitly anonymous. Role checks use [Authorize(Roles="Admin")] or policies reflecting Keycloak roles.

- On the frontend, a user will log in via a Keycloak login page (redirect or using an OIDC client). After login, the Keycloak-issued token is stored (e.g. in an AuthContext) and attached to API requests. The app can use Keycloak's JavaScript adapter or a library like @react-keycloak/web for integration. The authentication context hook (e.g. useAuth) wraps API calls so that services always include the current token.

**Frontend Design Patterns**

- **Folder Structure (src/)**: ZenFlow will use a structured, domain-driven folder organization with:
  * `app/` - Next.js App Router pages and layouts
  * `components/` - UI components organized into ui/, features/, and layouts/
  * `hooks/` - Custom React hooks
  * `lib/` - Technical infrastructure setup
  * `store/` - Zustand stores for state management
  * `services/` - API and business logic
  * `utils/` - Pure utility functions
  * `styles/` - Global styles and theme configuration

- **Visual Workflow Editor**: The core workflow editor will be built using React Flow, with the following components:
  * `WorkflowCanvas` - The main editor container that manages the React Flow instance
  * `CustomNodes` - Domain-specific node types (triggers, actions, conditions, etc.)
  * `CustomEdges` - Styled edge connections between nodes
  * `NodePanel` - Configuration panel for the selected node using React Hook Form and Chakra UI
  * `ToolboxPanel` - Draggable node types that can be added to the canvas
  * `WorkflowControls` - Buttons for saving, running, and managing workflows

- **State Management with Zustand**: State will be managed with modular Zustand stores instead of Context or Redux:
  * `workflowStore` - Manages the current workflow definition, nodes, and edges
  * `executionStore` - Tracks workflow execution state and history
  * `authStore` - Manages authentication state and user information
  * `uiStore` - Controls UI state like sidebars, panels, and theme

- **Data Fetching with TanStack React Query**: API interactions will use React Query:
  * Defined query and mutation hooks (e.g., `useWorkflows`, `useCreateWorkflow`)
  * Automatic caching and invalidation for efficient data fetching
  * Background data synchronization and optimistic updates
  * Error handling and retry mechanisms

- **API Communication with Axios**: A configured Axios instance will handle all HTTP requests:
  * Base URL configuration based on environment
  * Authentication interceptors to attach JWT tokens to requests
  * Response interceptors for error handling
  * Request/response transformers for data formatting

- **Form Handling with React Hook Form**: Forms throughout the application, especially for node configuration:
  * Form validation using Zod or Yup schemas
  * Field-level validation with immediate feedback
  * Dynamic form generation based on node types
  * Form state persistence during canvas interactions

- **UI Components with Chakra UI**: A comprehensive UI layer using Chakra UI:
  * Custom theme extending Chakra's base with ZenFlow's colors and styles
  * Responsive layout components using Chakra's Grid and Flex
  * Accessible form components with built-in validation states
  * Modal dialogs, toasts, and popovers for interactive UI elements

- **Notification System with React Hot Toast**: User feedback will be provided through:
  * Success notifications after saving or executing workflows
  * Error notifications with helpful context
  * Warning notifications for potential issues
  * Info notifications for system events

- **Custom Hooks**: Application-specific hooks will abstract common behaviors:
  * `useWorkflowEditor` - Manages the React Flow instance and editor state
  * `useNodeTypes` - Provides available node types and their configurations
  * `useWorkflowExecution` - Controls workflow execution and monitors progress
  * `useAuth` - Handles authentication state and token refresh

- **Services Layer**: Backend communication will be organized into service modules:
  * `workflowService` - CRUD operations for workflows
  * `nodeTypeService` - Fetches available node types and their configurations
  * `executionService` - Controls workflow execution and fetches results
  * `authService` - Handles authentication and user management

This architecture provides a clean separation of concerns while enabling the complex interactions needed for a workflow automation tool. Each technology was selected for its specific strengths: React Flow for the graph editor, Zustand for lightweight state management, TanStack Query for efficient data fetching, React Hook Form for form handling, Chakra UI for accessible components, and React Hot Toast for user notifications.

# 6. Development Stages & Tasks

Development will proceed in iterative stages, from a **Minimum Viable Product (MVP)** to advanced features. Each stage builds on the previous, with clear deliverables. The goal is to allow a junior developer to follow along step-by-step.

## Stage 1: Project Initialization and Core Setup
**Purpose:** Setup the base monorepo, frontend, backend, CI/CD.
**Tasks:**
* Create monorepo structure (`apps/frontend`, `apps/backend/src`, `libs/shared`, etc.)
* Initialize **Next.js (TypeScript)** project in `/apps/frontend`
* Initialize **.NET 8 Modular Monolith** project in `/apps/backend/src`
* Create a **shared library** in `libs/` for DTOs and API contracts (optional at first)
* Create `.env.example` for frontend and backend
* Set up **CI/CD basic workflow** (GitHub Actions: checkout → build frontend → build backend)
* Set up Docker Compose file for local services (Postgres, Redis, RabbitMQ, Keycloak)
* Create README.md with project setup instructions

**Expected Output:** ✔️ Monorepo ready ✔️ Frontend and Backend apps scaffolded ✔️ Build pipeline green ✔️ Basic README written

## Stage 2: Authentication and Authorization Integration
**Purpose:** Add Keycloak authentication for frontend and backend.
**Tasks:**
* Deploy Keycloak locally via Docker
* Create a **realm**, **clients** (frontend), **roles** (admin/user) in Keycloak
* Configure **JWT Bearer Authentication** in .NET backend
* Configure **NextAuth.js** in frontend to connect to Keycloak (OIDC provider)
* Implement **useAuth** hook + **AuthContext** in frontend
* Create **protected routes** in frontend (redirect to login if unauthenticated)
* Secure basic API endpoints with `[Authorize]` attributes

**Expected Output:** ✔️ User can log in via Keycloak ✔️ Frontend receives valid JWT tokens ✔️ Backend authorizes API calls ✔️ Roles (admin/user) available in frontend session

## Stage 3: Basic CRUD APIs and Frontend Pages
**Purpose:** Build first real modules: User Management and Workflows.
**Tasks:**
* Backend:
   * Create **User** Entity and basic CRUD handlers (CreateUserCommand, GetUserQuery, etc.)
   * Create **Workflow** Entity and basic CRUD handlers (CreateWorkflowCommand, GetWorkflowQuery, etc.)
* Frontend:
   * Create pages: `/users`, `/workflows`
   * Implement Zustand stores (`authStore`, `workflowStore`)
   * Fetch data with React Query (`useUsers`, `useWorkflows`)
   * Create forms with React Hook Form for CreateUser and CreateWorkflow
   * Show toasts (success/error) with React Hot Toast

**Expected Output:** ✔️ Admin can create and view users ✔️ User can create and view workflows ✔️ Pages protected by auth ✔️ Good loading and error UX

## Stage 4: Visual Workflow Editor MVP
**Purpose:** Build basic drag-and-drop workflow editor using React Flow.
**Tasks:**
* Setup React Flow canvas inside `/workflows/[workflowId]/edit`
* Implement basic node types (TriggerNode, ActionNode, ConditionNode)
* Enable drag-drop from Toolbox → Canvas
* Enable connecting nodes (edges)
* Persist workflow graph state to backend (nodes and edges)
* Add Zustand store (`workflowEditorStore`) to manage editor state

**Expected Output:** ✔️ User can visually create workflows ✔️ Save/load workflows to/from database ✔️ Clean and responsive editor UI

## Stage 5: Workflow Execution and History
**Purpose:** Enable workflows to be executed and track results.
**Tasks:**
* Backend:
   * Implement WorkflowExecutor service
   * Execute workflows step-by-step based on nodes and edges
   * Record execution logs into DB (status, start time, end time, error if any)
* Frontend:
   * Create execution panel showing current execution status
   * Allow manual "Run" button from the editor
   * View workflow execution history (list of past runs)

**Expected Output:** ✔️ User can run workflows ✔️ User can see history of past runs ✔️ Errors/success status visible

## Stage 6: Notifications and Messaging
**Purpose:** Implement event-driven architecture (publish/subscribe).
**Tasks:**
* Configure MassTransit with RabbitMQ in backend
* Implement Outbox pattern (store outgoing messages in DB)
* Publish events (e.g., `WorkflowCreatedEvent`, `WorkflowExecutedEvent`)
* Create a Notifications module:
   * Subscribe to events
   * Show notifications in frontend (e.g., workflow run completed)

**Expected Output:** ✔️ Modules communicate via RabbitMQ ✔️ Notifications shown in frontend based on backend events

## 7. Appendix: References and Standards

- **Architecture Patterns**: We follow principles from Domain-Driven Design (DDD) and Clean Architecture. The use of **vertical slices** means each feature folder contains everything needed for that feature.

- **Modular Monolith**: The system is a monolith broken into modules; as one reference explains, modular monoliths "involve dividing the system into a set of loosely-coupled modules, each with a well-defined boundary". This isolates feature development and paves the way to microservices if needed.

- **Frontend Best Practices**: The chosen Next.js structure (using app/, components/, lib/, utils/ etc.) is based on community guidelines for large projects.

- **Messaging & Outbox**: We will use established patterns for reliability: the transactional outbox has been described in MassTransit's documentation. This ensures that database updates and outgoing messages are coordinated. MassTransit and RabbitMQ give us flexibility to grow beyond a single application.

This SRS provides a comprehensive roadmap for building ZenFlow. Each requirement and architectural choice is designed to ensure code clarity, scalability, and maintainability. By following the stages and referencing the cited best practices and patterns, the development team (including interns/juniors) can steadily build a professional-grade system.