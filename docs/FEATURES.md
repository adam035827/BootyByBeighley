# Features

This document catalogs the features built in **Booty by Beighley**.
Add entries here when new features are scaffolded or architectural patterns are introduced.
For the full product definition and planned features see `docs/APP.md`.

---

## Backend

### Clean Architecture layer scaffold
- **Layer**: backend
- **Pattern**: Clean Architecture (Domain / Application / Infrastructure / API)
- **Description**: Four-project solution with enforced dependency direction. Domain has no external dependencies. Application depends only on Domain. Infrastructure implements Application interfaces. API wires everything together.
- **Key files**:
  - `src/ModernApp.Domain/Common/Entity.cs`
  - `src/ModernApp.Domain/Common/ValueObject.cs`
  - `src/ModernApp.Application/Common/Interfaces/ICqrs.cs`
  - `src/ModernApp.Application/DependencyInjection.cs`
  - `src/ModernApp.Infrastructure/DependencyInjection.cs`
  - `src/ModernApp.Api/Program.cs`

---

### CQRS interfaces
- **Layer**: backend — Application
- **Pattern**: CQRS via pure DI (no mediator library)
- **Description**: Marker interfaces `ICommand<T>`, `IQuery<T>`, `ICommandHandler<TCommand, TResponse>`, `IQueryHandler<TQuery, TResponse>` enforce Command/Query separation. Handlers are injected directly into Minimal API endpoints — no `ISender` or dispatch bus required. FluentValidation is not used — validation is done via .NET 10 native data annotations.
- **Key files**:
  - `src/ModernApp.Application/Common/Interfaces/ICqrs.cs`

---

### PostgreSQL + EF Core infrastructure base
- **Layer**: backend — Infrastructure
- **Pattern**: Repository pattern over EF Core with Npgsql
- **Description**: `AppDbContext` provides the EF Core entry point for all database access. All entity configurations use Fluent API in `Infrastructure/Persistence/Configurations/`. Repository implementations live in `Infrastructure/Repositories/`. Schema is managed via EF Core migrations. The Cosmos DB infrastructure from the original template has been removed.
- **Key files**:
  - `src/ModernApp.Infrastructure/Persistence/AppDbContext.cs`
  - `src/ModernApp.Infrastructure/DependencyInjection.cs`

---

### Azure AD B2C authentication
- **Layer**: backend — API
- **Pattern**: Stateless token-based auth via Azure AD B2C
- **Description**: Azure AD B2C issues JWTs validated by the JWT Bearer middleware in `Program.cs`. A custom `role` claim distinguishes `Coach` from `Student`. The Coach role is a database flag — not hardcoded — so additional coaches can be granted access without code changes. Endpoints are protected with `.RequireAuthorization()` and role-based policies.
- **Key files**:
  - `src/ModernApp.Api/Program.cs`

---

### ProblemDetails error handling
- **Layer**: backend — API
- **Pattern**: RFC 9457 ProblemDetails
- **Description**: All API errors return structured ProblemDetails responses. The built-in .NET 10 exception handler middleware is enabled. Custom exception mappers can be added via `IExceptionHandler` implementations.
- **Key files**:
  - `src/ModernApp.Api/Program.cs`
  - `src/ModernApp.Api/Middleware/ExceptionHandlerExtensions.cs`

---

### NSwag API client generation
- **Layer**: cross-cutting (backend → frontend)
- **Pattern**: Code generation from OpenAPI spec
- **Description**: The backend exposes an OpenAPI document via `app.MapOpenApi()`. NSwag reads that spec and generates strongly typed TypeScript client classes and DTO interfaces into `frontend/app/src/app/core/api/`. Feature code imports those generated clients — no handwritten `HttpClient` services or DTO interfaces for backend data.
- **Key files**:
  - `nswag.json` — NSwag configuration (input: OpenAPI spec URL, output: `core/api/`)
  - `src/ModernApp.Api/Program.cs` — `app.MapOpenApi()` exposes the spec
  - `src/app/core/api/` — generated output (do not edit manually)
- **Regeneration**: `nswag run nswag.json` from the repo root whenever a backend DTO or endpoint changes

---

## Frontend

### Angular 21 standalone app scaffold
- **Layer**: frontend
- **Pattern**: Standalone components, no NgModules
- **Description**: Angular 21 application with standalone components, SCSS, routing, and `HttpClient` wired with the auth interceptor. `OnPush` change detection is the default for all components.
- **Key files**:
  - `src/app/app.config.ts`
  - `src/app/app.routes.ts`

---

### SCSS design system
- **Layer**: frontend
- **Pattern**: Design tokens as CSS custom properties + BEM naming
- **Description**: All design values (colors, spacing, typography, shadows, z-index, transitions) are defined as CSS custom properties in `_tokens.scss`. Components consume tokens via `var(--token-name)` — no raw values in component SCSS.
- **Key files**:
  - `src/styles/_tokens.scss`
  - `src/styles/_reset.scss`
  - `src/styles/_typography.scss`
  - `src/styles.scss`

---

### Auth interceptor
- **Layer**: frontend — core
- **Pattern**: `HttpInterceptorFn`
- **Description**: Functional HTTP interceptor that attaches the JWT access token from `sessionStorage` to all outgoing API requests. Registered in `app.config.ts` via `provideHttpClient(withInterceptors([...]))`.
- **Key files**:
  - `src/app/core/interceptors/auth.interceptor.ts`
  - `src/app/app.config.ts`

---

### ProblemDetails error service
- **Layer**: frontend — core
- **Pattern**: Injectable service
- **Description**: Parses `ProblemDetails` responses from the backend and converts them into user-friendly messages and field-level error maps. Used in feature services and components to display error state.
- **Key files**:
  - `src/app/core/services/error.service.ts`
  - `src/app/core/models/problem-details.model.ts`

---

### Feature-based folder structure
- **Layer**: frontend
- **Pattern**: Feature modules with lazy loading
- **Description**: The `features/` directory holds one subfolder per domain feature, each with its own component, service, routes, and SCSS. Features are lazy-loaded via `loadComponent` / `loadChildren` to keep the initial bundle small.
- **Key files**:
  - `src/app/features/` (placeholder — features added per fork)
  - `src/app/shared/index.ts`

---

## Example — TodoItem CRUD

A complete worked example of the CRUD pattern, spanning all layers.

### TodoItem — backend
- **Layer**: backend (Domain + Application + Infrastructure + API)
- **Pattern**: CQRS commands/queries, Cosmos DB repository, Minimal API endpoint group
- **Description**: Full create/read/update/delete implementation for a `TodoItem` entity. Demonstrates the end-to-end pattern: domain entity with factory methods, Application commands and queries each containing their handler, a Cosmos DB repository, and a `MapGroup`-based Minimal API endpoint file.
- **Key files**:
  - `src/ModernApp.Domain/TodoItems/TodoItem.cs`
  - `src/ModernApp.Application/Features/TodoItems/ITodoItemRepository.cs`
  - `src/ModernApp.Application/Features/TodoItems/TodoItemDto.cs`
  - `src/ModernApp.Application/Features/TodoItems/Commands/CreateTodoItemCommand.cs`
  - `src/ModernApp.Application/Features/TodoItems/Commands/UpdateTodoItemCommand.cs`
  - `src/ModernApp.Application/Features/TodoItems/Commands/DeleteTodoItemCommand.cs`
  - `src/ModernApp.Application/Features/TodoItems/Queries/GetTodoItemQuery.cs`
  - `src/ModernApp.Application/Features/TodoItems/Queries/GetTodoItemsQuery.cs`
  - `src/ModernApp.Infrastructure/Repositories/TodoItemRepository.cs`
  - `src/ModernApp.Api/Features/TodoItems/TodoItemEndpoints.cs`

### TodoItem — backend tests
- **Layer**: backend — tests
- **Pattern**: xUnit + NSubstitute handler tests
- **Description**: Each command and query handler is tested in isolation. The `ITodoItemRepository` interface is substituted with NSubstitute. Tests cover success paths, not-found cases, and state transitions (`Complete`, `Reopen`).
- **Key files**:
  - `tests/ModernApp.Application.Tests/Features/TodoItems/Commands/CreateTodoItemCommandHandlerTests.cs`
  - `tests/ModernApp.Application.Tests/Features/TodoItems/Commands/UpdateTodoItemCommandHandlerTests.cs`
  - `tests/ModernApp.Application.Tests/Features/TodoItems/Commands/DeleteTodoItemCommandHandlerTests.cs`
  - `tests/ModernApp.Application.Tests/Features/TodoItems/Queries/GetTodoItemQueryHandlerTests.cs`
  - `tests/ModernApp.Application.Tests/Features/TodoItems/Queries/GetTodoItemsQueryHandlerTests.cs`

### TodoItem — frontend
- **Layer**: frontend — features
- **Pattern**: Standalone components + Signals + reactive forms + lazy routing + NSwag-generated API client
- **Description**: `TodoListComponent` renders the list with signal-driven state (`items`, `isLoading`, `errorMessage`, `pendingItemCount`). `TodoFormComponent` handles both create and edit modes; the `:id` route parameter is bound to a signal `input()` via `withComponentInputBinding()`. Both components inject `TodoItemsClient` from the NSwag-generated `core/api/api.ts` directly — no handwritten service layer. Both use `ChangeDetectionStrategy.OnPush`.
- **Key files**:
  - `src/app/core/api/api.ts` — NSwag-generated client (`TodoItemsClient`, `TodoItemDto`, `CreateTodoItemCommand`, `UpdateTodoItemRequest`)
  - `src/app/features/todo-items/components/todo-list/todo-list.component.ts`
  - `src/app/features/todo-items/components/todo-form/todo-form.component.ts`
  - `src/app/features/todo-items/todo-items.routes.ts`

### TodoItem — frontend tests
- **Layer**: frontend — tests
- **Pattern**: Vitest (unit/component) + Playwright (E2E)
- **Description**: `TodoListComponent` and `TodoFormComponent` are tested at multiple levels. Unit and component tests use Vitest with Angular's `TestBed` for isolated testing of logic, signals, and DOM behavior. E2E tests use Playwright for full browser automation and workflow validation. Signal values are asserted by calling them as functions (`component.items()`). The edit-mode spec sets a signal input via `fixture.componentRef.setInput()`. There is no handwritten service to test separately.
- **Key files**:
  - `e2e/todo-items.spec.ts` (Playwright E2E tests)
  - `src/app/features/todo-items/components/todo-list/todo-list.component.spec.ts` (Vitest component tests)
  - `src/app/features/todo-items/components/todo-form/todo-form.component.spec.ts` (Vitest component tests)
