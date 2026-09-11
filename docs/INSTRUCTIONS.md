# Instructions

This document is the **architectural source of truth** for Booty by Beighley.
For the full product definition, features, and decisions see `docs/APP.md`.

---

## Local development

### Prerequisites

| Tool | Install |
|---|---|
| .NET 10 SDK | https://dot.net |
| Node.js 22 LTS | https://nodejs.org |
| Angular CLI 21 | `npm install -g @angular/cli` |
| NSwag CLI | `npm install -g nswag` |
| PostgreSQL 16 (local) | https://www.postgresql.org/download/ or Docker |

### Secrets

The backend requires the following secrets via [.NET user secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) — never committed to source control.

```powershell
cd backend/src/BootyByBeighley.Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<postgres-connection-string>"
dotnet user-secrets set "AzureAdB2C:ClientSecret" "<b2c-client-secret>"
dotnet user-secrets set "Azure:BlobStorage:ConnectionString" "<blob-storage-connection-string>"
```

### Running

Use the **Run Application** compound launch configuration in VS Code (`.vscode/launch.json`). It:
1. Kills any process already bound to port 5118, builds, and starts the .NET backend on `http://localhost:5118` (coreclr debugger attached)
2. Waits for the backend to respond, then runs `nswag run nswag.json` to regenerate the TypeScript client
3. Kills any process already bound to port 4200, then starts `ng serve --host 127.0.0.1` at `http://127.0.0.1:4200`
4. On stop, kills the frontend dev server automatically (`postDebugTask: kill: frontend`)

The Angular dev server proxies `/api/*` and `/openapi/*` to the backend via `frontend/app/proxy.conf.json`.

### Regenerating the NSwag client

Whenever the backend API changes, regenerate the TypeScript client:
```powershell
# From repo root, with the backend running:
nswag run nswag.json
```
The generated file is `frontend/app/src/app/core/api/api.ts`. **Do not edit it manually.**

---

## Stack versions

| Layer | Technology | Version |
|---|---|---|
| Backend runtime | .NET | 10 |
| Backend API style | Minimal APIs | .NET 10 built-in |
| Backend CQRS | Pure DI (native interfaces) | — |
| Backend validation | .NET 10 native (`[Required]`, etc.) | — |
| Backend persistence | PostgreSQL via EF Core + Npgsql | Latest stable |
| Backend auth | Azure AD B2C + JWT Bearer | — |
| Backend video storage | Azure Blob Storage SDK | Latest stable |
| Frontend framework | Angular | 21 |
| Frontend mobile wrapper | Capacitor | Latest stable |
| Frontend state | Angular Signals | Built-in |
| Frontend styling | SCSS + CSS custom properties | — |
| API client generation | NSwag | Latest |
| IaC | Bicep | Latest |
| Cloud platform | Azure | — |
| Node.js | Node.js LTS | 22 |

---

## Folder structure and boundaries

```
Booty-by-Beighley/
  backend/
    src/
      BootyByBeighley.Domain/          # Entities, value objects — no external deps
      BootyByBeighley.Application/     # CQRS handlers, DTOs, interfaces
      BootyByBeighley.Infrastructure/  # EF Core DbContext, PostgreSQL repos, Blob Storage
      BootyByBeighley.Api/             # Minimal API endpoints, middleware, DI wiring
    tests/
      BootyByBeighley.Domain.Tests/
      BootyByBeighley.Application.Tests/
      BootyByBeighley.Infrastructure.Tests/
  frontend/
    app/src/app/
      core/           # Interceptors, guards, services, auth (Azure AD B2C)
      shared/         # Reusable components, pipes, directives
      features/       # One folder per domain feature (lazy-loaded)
      styles/         # SCSS design tokens, reset, typography
  infrastructure/
    modules/          # Bicep modules (app, database, storage, b2c, keyvault, networking)
    environments/     # Per-environment parameter files
    main.bicep
  docs/               # APP.md, INSTRUCTIONS.md, FEATURES.md, AGENTS.md
  .github/agents/     # Agent prompt files
```

**Rules:**
- Backend code belongs only in `backend/`
- Frontend code belongs only in `frontend/`
- IaC belongs only in `infrastructure/`
- Documentation belongs only in `docs/`
- Never mix concerns across these boundaries

---

## Backend patterns

### Clean Architecture — dependency direction

```
Domain ← Application ← Infrastructure
                    ← API
```

- **Domain**: pure business logic, no framework dependencies
- **Application**: commands, queries, handlers, interfaces — depends only on Domain
- **Infrastructure**: implements Application interfaces, contains all Cosmos DB access
- **API**: Minimal API endpoints — no business logic, only HTTP concerns

### CQRS

Every feature uses the Command/Query pattern with native DI — no mediator library required:

```csharp
// Command (write)
public record CreateItemCommand([Required] string Name) : ICommand<CreateItemResponse>;

// Query (read)
public record GetItemByIdQuery([Required] string Id) : IQuery<ItemResponse?>;
```

Each Command/Query has:
- Request record (with validation attributes) **and its handler co-located in the same `.cs` file**
- Handler implements `ICommandHandler<,>` or `IQueryHandler<,>`
- Response DTO (never expose domain entities directly)

### Validation

Use .NET 10 native validation — **FluentValidation is banned**:

```csharp
public record CreateItemCommand(
    [Required][StringLength(200)] string Name,
    [Range(0, int.MaxValue)] int Quantity
) : ICommand<CreateItemResponse>;
```

- Enable endpoint validation: `.WithParameterValidation()`
- For complex rules: implement `IValidatableObject` on the request record
- Validation failures return 400 with field-level ProblemDetails

### Error handling

All errors use RFC 9457 ProblemDetails:

```csharp
return Results.Problem(
    detail: "Item not found.",
    instance: context.Request.Path,
    statusCode: StatusCodes.Status404NotFound,
    title: "Not Found",
    type: "https://tools.ietf.org/html/rfc9110#section-15.5.5"
);
```

Required fields: `type`, `title`, `status`, `detail`, `instance`, `traceId`

### Minimal API endpoints

```csharp
// Feature endpoint group — no business logic here
app.MapPost("/items", async (CreateItemCommand cmd, ICommandHandler<CreateItemCommand, CreateItemResponse> handler, CancellationToken ct) =>
{
    var result = await handler.ExecuteAsync(cmd, ct);
    return TypedResults.Created($"/items/{result.Id}", result);
}).WithParameterValidation().RequireAuthorization();
```

### Persistence abstraction

All data access is hidden behind a repository interface defined in the **Application** layer. The concrete implementation lives in **Infrastructure** and is registered in `DependencyInjection.cs`. No Cosmos SDK types appear in Application or Domain.

```
Application/
  Features/TodoItems/
    ITodoItemRepository.cs   ← interface (domain types only)

Infrastructure/
  Persistence/
    CosmosDocument.cs        ← Cosmos base document (internal)
  Repositories/
    TodoItemRepository.cs    ← Cosmos implementation
    TodoItemDocument.cs      ← Cosmos document type (internal)
    TodoItemMappings.cs      ← domain ↔ document mapping (internal)
  DependencyInjection.cs     ← THE ONLY file that references CosmosClient
```

#### Swapping the database

To replace Cosmos DB with another store (e.g. EF Core + SQL, MongoDB):
1. Replace the `PackageReference` in `BootyByBeighley.Infrastructure.csproj`
2. Rewrite `DependencyInjection.cs` to register the new client and repository
3. Replace the repository implementation — implement the same `ITodoItemRepository<T>` interface
4. Delete/replace the document and mapping files
5. **Nothing in Domain or Application changes**

The `ITodoItemRepository` interface is the seam. Keep all persistence types (`*Document`, `CosmosDocument`, mapping helpers) `internal` to Infrastructure.

---

### Authentication

- Stateless JWT Bearer tokens
- Configured via `builder.Services.AddAuthentication().AddJwtBearer()`
- JWT configuration (issuer, audience, signing key) comes from Key Vault / environment config
- Use `[Authorize]` / `.RequireAuthorization()` on all protected endpoints

---

## Frontend patterns

### Standalone components

All components are standalone — no NgModules:

```typescript
@Component({
  selector: 'app-item-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './item-list.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
```

### Signals

```typescript
// Local state
readonly items = signal<Item[]>([]);
readonly isLoading = signal(false);

// Derived state
readonly itemCount = computed(() => this.items().length);

// Side effects (use sparingly)
effect(() => console.log('items changed:', this.items()));
```

- Never use `BehaviorSubject` for state — use Signals
- `toSignal()` to convert Observables from HTTP calls

### Feature structure

```
features/
  {feature-name}/
    models/                          # TypeScript interfaces matching backend DTOs
    services/
      {feature-name}.service.ts      # HttpClient wrappers for this feature
    components/
      {component-name}/
        {component-name}.component.ts
        {component-name}.component.html
        {component-name}.component.scss
    {feature-name}.routes.ts         # Lazy-loaded route definitions
```

Simple single-screen features may keep everything flat. Use sub-folders when there are multiple components or models.

### SCSS

- All design values come from tokens in `styles/_tokens.scss`
- Use BEM naming: `.block__element--modifier`
- Mobile-first: base styles for small screens, `@media (min-width: ...)` for larger breakpoints
- Breakpoints: `sm: 640px`, `md: 768px`, `lg: 1024px`, `xl: 1280px`

### Route parameters as signal inputs

Enable `withComponentInputBinding()` in `app.config.ts` so route `:param` values are bound directly to component signal `input()` fields:

```typescript
// app.config.ts
provideRouter(routes, withComponentInputBinding())

// component
readonly id = input<string | undefined>(undefined);
readonly isEditMode = computed(() => !!this.id());
```

The input name must match the route parameter name exactly. Use `fixture.componentRef.setInput('id', value)` in tests.

---

## Frontend–backend integration

API clients and DTO interfaces are **code-generated from the backend's OpenAPI spec** using **NSwag**. Do not handwrite `HttpClient` wrapper services or DTO interfaces for backend data — generate them instead.

### How it works

1. The backend exposes an OpenAPI document via `app.MapOpenApi()` in `Program.cs`.
2. NSwag reads that spec and generates a typed TypeScript client + interfaces into `frontend/app/src/app/core/api/`.
3. Feature services import and use the generated client classes — they do **not** call `HttpClient` directly.

### Generation command

```bash
# Run from the repo root — requires the backend to be running (or point at the spec JSON)
nswag run nswag.json
```

### Rules
- **Never manually edit** files inside `core/api/` — they are overwritten on every generation.
- Re-run generation whenever a backend DTO or endpoint signature changes.
- Commit the generated files so the frontend compiles without requiring a running backend.
- The NSwag config file lives at `nswag.json` in the repo root.

---

## Infrastructure

- All IaC in Bicep, targeting Azure
- Modular: separate Bicep module per resource type
- Multi-environment: `environments/{dev|stage|prod}.bicepparam`
- No hard-coded secrets — all secrets in Key Vault
- Use managed identities for service-to-service authentication

---

## Testing

### Backend
- **xUnit** for all tests
- **NSubstitute** for mocking (not Moq)
- Test naming: `Method__GivenContext__ShouldOutcome`

### Frontend — Testing Pyramid

The project uses **both Vitest and Playwright** for different layers of the testing pyramid. They serve different purposes and must never overlap.

#### 🟩 Unit Tests → Vitest
- Pure TypeScript logic
- Angular services
- Signals state logic
- Utility functions
- Pipes
- Component logic (shallow tests)

#### 🟨 Component Tests → Vitest
- Angular component tests with `TestBed`
- DOM behavior in jsdom
- Input/output bindings
- Template logic
- Component-level state

#### 🟥 E2E / UI Automation → Playwright
- Full end-to-end tests
- Real browser automation
- Cross-browser testing (Chromium, Firefox, WebKit)
- Mobile emulation
- Capacitor webview testing
- Full workflow validation

**Why both?**
- Vitest = fast, isolated, logic-level tests
- Playwright = slow, realistic, full-stack tests
- Angular 21 guidance: Vitest for unit/component, Playwright for E2E
- Capacitor mobile builds require Playwright's real browser capabilities

**Rules:**
- Unit and component tests → Vitest only
- E2E and UI automation → Playwright only
- Never use Playwright for component logic testing
- Never use Vitest for real browser workflows
