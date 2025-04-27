# Software Requirements Specification (SRS): ZenFlow

## 1. Introduction

**ZenFlow** is a modern web platform for managing organizational workflows and tasks. It will enable users to create, assign, and track tasks ("flows") in a flexible, secure way. The system will be built as a **full-stack application**: a **Next.js** (React) frontend and a **.NET 8** backend. This SRS outlines the system's scope, architecture, and requirements, emphasizing modular, scalable design. The frontend and backend will live in a **monorepo**, with shared libraries for common code (e.g. types, models). The design adopts a **modular monolith** backend architecture (a "Modulith") -- i.e., a single deployed application divided into independent modules -- which "**takes the best of both worlds**" of monoliths and microservices. Each module will use **Vertical Slice Architecture** (feature folders) with Domain-Driven Design and CQRS via MediatR to isolate functionality by use case. Authentication and authorization will use **OAuth2/OpenID Connect** via Keycloak, providing single sign-on, user management and fine-grained roles. Message-based, asynchronous communication will use RabbitMQ with MassTransit, implementing the **Transactional Outbox Pattern** to ensure reliability. PostgreSQL (with EF Core code-first) will store data, and Redis will act as a distributed cache to improve performance.

## 2. System Overview

ZenFlow consists of two main components: the **Frontend** (Next.js app) and the **Backend** (ASP.NET Core Modulith). They share code in a monorepo. The frontend will be structured around **Next.js App Router** (or Pages Router) with separate folders for features, components, hooks, contexts, and services. For example, a typical structure might be:

```
/src
  /app or /pages # Next.js routes
  /components    # Reusable React components
  /contexts      # React Context providers (e.g. AuthContext, ThemeContext)
  /hooks         # Custom hooks (e.g. useAuth, useApi)
  /services or /lib # API client code, business logic utilities
  /styles        # Global and component styles
  /public        # Public assets
```

A core principle is "separation of concerns": UI components live under components/, application logic like API calls or auth helpers go into a services/ or lib/ folder, and global state (contexts or stores) in contexts/. Utility functions (pure helpers) belong in utils/ or helpers/. For example, authentication helpers and API client config would reside in a lib/ directory, while pure utils (date formatting, validation) go into utils/. The official Next.js guide similarly recommends a structure with app/, components/, lib/, utils/, and styles/ folders.

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

- **Workflow/Task Management (FR3)**: ZenFlow's core feature is allowing users to **create and manage workflows** (a "flow" consists of tasks or steps). Users can perform CRUD operations on workflows. For example, a user can create a new workflow with specific steps, assign tasks to users, and mark tasks complete. Each workflow is a domain entity stored in PostgreSQL. These actions are exposed via RESTful endpoints in the corresponding backend module and via pages/components on the frontend.

- **Asynchronous Notifications (FR4)**: When certain events occur (e.g. a new workflow is created or a task is completed), the system shall publish a domain event message. For example, creating a workflow publishes a WorkflowCreated event to RabbitMQ. Other modules (or future services) may subscribe to these events. Event publishing uses the Outbox pattern so that, e.g., creating a workflow in the database and enqueuing the message occur in one transaction.

- **Data Consistency and Reliability (FR5)**: The system shall use **PostgreSQL** with EF Core for persistent storage of all domain data. Operations that affect multiple tables or modules (e.g. an order that affects inventory and accounts) must be handled transactionally within the monolith or via eventual consistency with messaging. The Outbox ensures message delivery even if the message broker or network is temporarily unavailable.

- **Caching (FR6)**: To improve read performance, ZenFlow shall cache frequently accessed data in **Redis**. For instance, user session information or reference data may be cached. The cache will be updated on data changes to ensure consistency.

- **Clean API Design (FR7)**: Backend APIs shall follow REST principles (or minimal API patterns) and use consistent request/response models. Data validation will be applied (e.g., model binding and FluentValidation or similar) so that invalid inputs are rejected.

- **Scalable Frontend (FR8)**: The Next.js frontend shall be modular. Components and hooks should be reusable across pages. State management (e.g. React Context or a lightweight store like Zustand) will be used for global state (authentication, theme, etc). For example, an AuthContext can provide the current user and auth token to components, avoiding prop-drilling. Service modules (e.g. API clients under lib/) will encapsulate calls to the backend.

- **Reusable Shared Code (FR9)**: Common code (e.g. TypeScript interfaces for API data, form components, validation schemas) will be placed in shared folders/packages to avoid duplication. For example, if the backend and frontend both use a UserDto definition, it can live in a shared libs/shared package in the monorepo.

- **Module Isolation (FR10)**: Internal backend modules shall be isolated. A module may expose internal services or a public API. Other modules should not directly access a module's database; all cross-module actions happen via messages or public APIs. This encapsulation allows each module to evolve independently.

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

- **Folder Structure (src/)**: As discussed, use app/ (or pages/), components/, contexts/, hooks/, lib/, utils/, and styles/.

- **Contexts & Hooks**: For global state (auth user, theme, etc.), React Contexts are defined under contexts/. Custom hooks under hooks/ consume these contexts or other logic (e.g. useFetch for calling the API). Services (in lib/ or services/) are plain JavaScript/TypeScript functions or classes that perform side effects -- e.g. apiClient.ts with functions like getWorkflows() or createWorkflow(). These services internally use fetch() or axios with the base API URL. They encapsulate endpoints, so components stay decoupled from raw API details. As recommended by best practices, the lib directory is for code that "interfaces with external services" or "contains business logic", e.g. API clients and auth helpers.

- **Component Structure**: Reusable UI components (buttons, form fields) go in components/. Page-specific components can also live with their pages if using App Router (i.e. inside app/dashboard/). Each feature page under app/ (like app/workflows/) can have nested folders or React Server Components if using Next.js 13+.

- **State Management**: We will likely use React Context for auth and any cross-cutting state, and possibly a simple state library (Zustand or Redux) if needed for complex global stores. As an example, an AuthProvider can use a hook to check token validity and refresh if needed. Data fetching can use React Query or SWR for caching API calls.

### 5.2 Folder and Code Organization

A possible **monorepo layout**:

```
/apps
  /frontend/  # Next.js application (package.json, tsconfig, etc.)
  /backend/   # .NET solution (ZenFlow.sln, projects per module)
/libs/        # Shared libraries/packages
  /models/    # DTOs/interfaces used by both FE and BE
  /ui/        # Shared React components or design system (if applicable)
/docker/      # Container/Docker configs, Compose files
/infra/       # Terraform or Kubernetes configs (if any)
/README.md
```

In the backend folder, each module is a C# class library or project (e.g. ZenFlow.Workflows, ZenFlow.Notifications, etc.) plus one Minimal API project that references them and wires up routes. Project references (or shared project files) ensure a clean modular build. Shared domain code or common utilities can live in a shared library (e.g. ZenFlow.Core).

On the frontend, workspaces (like PNPM or Yarn workspaces) can link shared libs/models into the Next app. This avoids duplicating type definitions for API models.

### 5.3 Technical Decisions and Rationale

- **Monorepo**: Eases synchronized development of FE/BE. Shared code (e.g. validation rules) can be in one place. Tools like Turborepo or Nx (or simple workspaces) will help.

- **Vertical Slice + DDD**: Organizing by feature keeps related code together and reduces merging conflicts for large teams. As Jimmy Bogard notes, vertical slices group all "concerns from front-end to back" around each request.

- **MassTransit**: Provides an abstraction over RabbitMQ and supports patterns like sagas. Using MassTransit outbox simplifies reliable messaging.

- **Redis**: Chosen as a high-performance cache. It's a common pair with .NET apps and easily integrates with the StackExchange.Redis library.

- **Keycloak**: Open-source and widely supported by Spring Boot and .NET; it handles user federation, SSO, social login, etc. It saves development time on auth.

- **Minimal APIs**: Reduces ceremony. Since we're using MediatR, endpoints can directly call await mediator.Send(new Command(...)), avoiding extra controller layers. This keeps the code concise and focused.

## 6. Development Stages & Tasks

Development will proceed in iterative stages, from a **Minimum Viable Product (MVP)** to advanced features. Each stage builds on the previous, with clear deliverables. The goal is to allow a junior developer to follow along step-by-step.

### Stage 1: Project Setup & MVP Foundation

- **Monorepo and Tooling**: Set up the monorepo with version control. Initialize Next.js app and .NET solution in separate folders. Configure TypeScript for FE and a C# solution (ZenFlow.sln) with ASP.NET Core. Choose a package manager (npm/Yarn/PNPM) and workspace config.

- **Basic Folder Structure**: In Next.js, create the core directories (app/, components/, lib/, utils/, contexts/). In .NET, create initial projects: a Web API project and placeholder class libraries for modules (e.g. ZenFlow.Core, ZenFlow.Users, ZenFlow.Workflows).

- **Keycloak Integration**: Install and configure Keycloak (could use Docker). Create a realm, client, and roles. In the backend, configure JWT authentication (OIDC) pointing to Keycloak. In the frontend, implement a login page or redirect flow to Keycloak. Ensure the frontend can obtain and store the token (e.g. in React Context).

- **Hello World Endpoints**: Create a simple Minimal API endpoint (e.g. /api/hello) to verify the pipeline. Protect it with [Authorize]. Make the frontend call this endpoint with the Keycloak token to display a message.

- **Database Setup**: Add EF Core packages. Configure a PostgreSQL database (local or Docker). Create an initial DbContext in ZenFlow.Core (or a specific module) and run a migration.

- **CI/CD Baseline**: (Optional) Set up a simple pipeline to build and test both apps. This could be a GitHub Actions workflow that builds the .NET solution and runs npm run build.

### Stage 2: Core Features and Module Development

- **User Module**: Implement the User Management module. Define a User entity (name, email, role) in EF Core. Create MediatR handlers for CreateUserCommand, GetUserQuery, etc. Expose corresponding API endpoints (e.g. POST /api/users). Make a frontend page for admin to list/add users. Use Keycloak admin API or database hooks to sync user creation with Keycloak (or rely on Keycloak solely).

- **Workflow Module**: Implement the core "Workflow" module. Define domain models (e.g. Workflow, Task). Create handlers and endpoints for creating and updating workflows. For example, POST /api/workflows sends a CreateWorkflowCommand to MediatR, which saves to DB and publishes a WorkflowCreated event via MassTransit.

- **Redis Caching**: Add Redis. For example, implement caching for expensive read queries. E.g., cache the result of GetAllWorkflows for 5 minutes. Use the StackExchange.Redis client. On write, invalidate relevant cache keys.

- **Asynchronous Messaging**: Install MassTransit and RabbitMQ. Configure the outbox on the Workflow module's DbContext. After saving a new workflow, publish an event message. Create a simple Notification module (or inside Workflow) that consumes this event and logs it or sends an email (simulated). Verify that messaging works end-to-end (e.g. by checking console logs or DB outbox tables).

- **Frontend Enhancements**: Build UI for workflows: list existing workflows, create new, mark complete. Use React Query or similar to fetch data from the new endpoints. Use components from components/ for forms/tables. Use contexts/hooks for auth and API calls (e.g. a useApi hook that adds auth header).

### Stage 3: Additional Features & Robustness

- **Notifications Module**: Develop a separate Notifications module that subscribes to events (user created, workflow completed). For example, upon receiving WorkflowCompleted, it might send an email (or simply log to console). Ensure each module has its own service registrations (DI) and message consumers.

- **Redis Session Store (optional)**: Use Redis not just for caching but also as a session store if using server components or to store token info.

- **Unit/Integration Tests**: Write tests for handlers and services. For example, test CreateWorkflowCommandHandler and a consumer for an event. Use an in-memory DB or test container for PostgreSQL.

- **Logging and Monitoring**: Integrate logging (Serilog) in .NET and ensure important actions (login attempts, errors, key events) are logged. Optionally configure health checks and a metrics endpoint (e.g. Prometheus exporter).

- **Frontend Polishing**: Add global styles (CSS or Tailwind). Ensure mobile responsiveness. Improve error handling (show notifications on save success/failure).

### Stage 4: Advanced/Optional Enhancements

- **CI/CD and Deployment**: Refine pipelines. Build Docker images for frontend and backend. Prepare Docker Compose or Kubernetes manifests to deploy the full stack (including Postgres, RabbitMQ, Redis, Keycloak).

- **Performance Testing**: Load test key endpoints and optimize. Possibly implement pagination and filtering for lists.

- **Modular Evolution**: Evaluate splitting modules into separate deploys (microservices). The design of Stage 2 modules should make this easier in the future. Document guidelines for creating new modules.

- **Security Hardening**: Implement rate limiting, content security policies, and other HTTP security headers. Apply Penetration Testing best practices.

Each stage's tasks build on previous work. A junior developer can start by following onboarding (Stage 1), then pick a simple CRUD feature (Stage 2), learn messaging (Stage 3), and finally infrastructure concerns (Stage 4).

## 7. Appendix: References and Standards

- **Architecture Patterns**: We follow principles from Domain-Driven Design (DDD) and Clean Architecture. The use of **vertical slices** means each feature folder contains everything needed for that feature.

- **Modular Monolith**: The system is a monolith broken into modules; as one reference explains, modular monoliths "involve dividing the system into a set of loosely-coupled modules, each with a well-defined boundary". This isolates feature development and paves the way to microservices if needed.

- **Frontend Best Practices**: The chosen Next.js structure (using app/, components/, lib/, utils/ etc.) is based on community guidelines for large projects.

- **Messaging & Outbox**: We will use established patterns for reliability: the transactional outbox has been described in MassTransit's documentation. This ensures that database updates and outgoing messages are coordinated. MassTransit and RabbitMQ give us flexibility to grow beyond a single application.

This SRS provides a comprehensive roadmap for building ZenFlow. Each requirement and architectural choice is designed to ensure code clarity, scalability, and maintainability. By following the stages and referencing the cited best practices and patterns, the development team (including interns/juniors) can steadily build a professional-grade system.