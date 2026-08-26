# Copilot Instructions — Booty by Beighley

You are working inside the **Booty by Beighley** codebase — a fitness instructing platform for a single coach and her students. The full product definition is in `docs/APP.md`. All code, plans, and reviews must follow the rules in this file and the docs in `/docs`.

Your priorities, in order:

1. **Obey the architecture and folder boundaries.**
2. **Preserve Clean Architecture and CQRS.**
3. **Build for the product defined in `docs/APP.md`.**
4. **Write clear, minimal, maintainable code.**

---

## 1. Project layout and boundaries

The repository is organized as:

- `backend/` — .NET 10, Minimal APIs, CQRS, Clean Architecture
- `frontend/` — Angular 21, standalone components, Signals, SCSS
- `infrastructure/` — Bicep IaC and deployment-related assets
- `docs/` — Architecture, features, and multi‑agent workflow docs

You must:

- Put **all backend code** in `backend/`
- Put **all frontend code** in `frontend/`
- Put **all IaC** in `infrastructure/`
- Put **all documentation** in `docs/`
- Never mix concerns across these folders

---

## 2. Backend architecture (.NET 10, Minimal APIs, CQRS)

Backend must follow:

- **.NET 10 Minimal APIs**
- **Clean Architecture**
- **CQRS with pure DI** — `ICommandHandler<,>` and `IQueryHandler<,>` injected directly into endpoints

Recommended layers (even if not physically separated yet):

- **Domain** — core business logic, entities, value objects, domain services
- **Application** — commands, queries, handlers, interfaces (no separate validator classes — validation is attributes on request records)
- **Infrastructure** — PostgreSQL (EF Core + Npgsql), Azure Blob Storage, external service implementations
  - **API** — Minimal API endpoints, request/response mapping

### Backend rules

- **Commands** for writes, **Queries** for reads.
- Each Command/Query has:
  - Request record with `.NET 10 native validation` attributes (`[Required]`, `[StringLength]`, `[Range]`, etc.) — **FluentValidation is banned**
  - Handler (co-located in the same file as the request record)
  - Response DTO (if needed — never expose domain entities directly)
- Minimal API endpoints:
  - Map to Commands/Queries by injecting `ICommandHandler<,>` or `IQueryHandler<,>` directly
  - Contain no business logic
  - Only handle HTTP concerns (status codes, mapping, ProblemDetails)
- Use **PostgreSQL via EF Core** for persistence:
  - All `DbContext` access and repository implementations live in Infrastructure
  - No direct EF Core or `DbContext` usage in Application, Domain, or API
- Do not leak domain entities directly to API responses; use DTOs.

---

## 3. Frontend architecture (Angular 21, Capacitor, Signals, SCSS)

Frontend must follow:

- **Angular 21**
- **Capacitor** — native iOS and Android wrapper
- **Standalone components**
- **Signals for state management**
- **SCSS with a design system approach**
- **Mobile-first, responsive design** — phone screen is the primary target

### Frontend rules

- Use **feature-based folders** (e.g., `features/xyz`, `shared/`, `core/`).
- Use **standalone components** (no NgModules).
- Use **Signals**:
  - `signal()` for local state
  - `computed()` for derived state
  - `effect()` only when necessary for side effects
- Use Angular’s modern control flow (`@if`, `@for`, etc.) where appropriate.
- Use **SCSS** with:
  - Design tokens (colors, spacing, typography)
  - BEM or similarly consistent naming
- Components should be:
  - Small
  - Focused
  - Composable
- Avoid global state unless truly necessary; prefer feature-scoped state.

---

## 4. Infrastructure (Bicep, Azure)

Infrastructure must:

- Use **Bicep** for Infrastructure-as-Code.
- Target **Azure** resources (Container Apps / App Service, PostgreSQL Flexible Server, Blob Storage, AD B2C, Key Vault).
- Be modular: separate modules for app, database, storage, auth, networking.
- Support multiple environments (dev/stage/prod) via parameters.

Do not hard-code secrets; assume they come from Key Vault or environment configuration.

---

## 5. Persistence and data modeling (PostgreSQL + EF Core)

When generating persistence logic:

- Use **PostgreSQL** via **EF Core** (Npgsql provider) as the primary data store.
- All entities are mapped as EF Core entities with explicit relationships and indexes.
- Manage schema changes with EF Core migrations (`dotnet ef migrations add`).
- The `AppDbContext` lives in `ModernApp.Infrastructure/Persistence/` — never reference it from Application, Domain, or API.
- Application layer depends only on abstractions (repository interfaces defined in Application, implemented in Infrastructure).
- All repository implementations live in `ModernApp.Infrastructure/Repositories/`.
- Define indexes explicitly and document their rationale in a comment.
- Keep document classes and mapping helpers `internal` to Infrastructure so they cannot leak across the boundary.

---

## 6. Error handling and ProblemDetails

All backend errors should use **ProblemDetails** (extended):

- Include:
  - `type`
  - `title`
  - `status`
  - `detail`
  - `instance` (path)
  - `traceId`
  - Optional: `errorCode`, `errors[]` for validation issues
- Validation failures:
  - Return 400 with a ProblemDetails payload containing field-level errors.
- Frontend:
  - Parse ProblemDetails
  - Show user-friendly messages
  - Never display raw stack traces or internal details

---

## 7. Authentication and security (generic)

Authentication should be:

- **Token-based** (e.g., JWT access tokens)
- **Stateless** on the server
- Ready for:
  - First-party login
  - Future OAuth providers (e.g., Google) as an extension

Security rules:

- Validate all inputs.
- Use proper authorization checks at the API boundary.
- Do not expose sensitive data in logs or responses.
- Follow OWASP best practices where applicable.

---

## 8. Frontend–backend integration

API clients and TypeScript DTO interfaces are **generated by NSwag** from the backend OpenAPI spec — never handwritten.

- The backend must expose its OpenAPI document via `app.MapOpenApi()` in `Program.cs`.
- NSwag reads the spec and generates typed clients + interfaces into `frontend/app/src/app/core/api/`.
- Feature components and services import and use those generated clients.
- Never call `HttpClient` directly in feature code — always go through the generated client.
- **Never manually edit** files in `core/api/` — they are regenerated and overwritten.
- Re-run `nswag run nswag.json` whenever a backend DTO or endpoint changes.
- Keep API base URLs configurable (environment-based).
- Always handle three states: loading, error (ProblemDetails), and data.
- Avoid duplicating business logic on the frontend; treat backend as the source of truth.

---

## 9. Code style and commenting philosophy

Code style:

- Prefer **clear naming** over comments.
- Keep functions small and focused.
- Avoid deeply nested logic; refactor into smaller units.
- Remove dead code and unused imports.
- Maintain consistent formatting.

Commenting philosophy:

- Do **not** comment obvious code.
- Use comments only when:
  - Explaining non-obvious decisions
  - Documenting constraints or tradeoffs
  - Clarifying why something is done, not what is done

---

## 10. Documentation files in `/docs`

The following files exist (or will exist) in `/docs`:

- `INSTRUCTIONS.md` — Detailed architecture and development rules.
- `FEATURES.md` — Exploratory and evolving product features (generic; app-specific in forks).
- `AGENTS.md` — Multi-agent development workflow (orchestrator, planners, implementers, reviewers, etc.).

When in doubt:

- Treat `INSTRUCTIONS.md` as the **architectural source of truth**.
- Treat `FEATURES.md` as the **feature and behavior catalog**.
- Treat `AGENTS.md` as the **process/workflow guide**.

---

## 11. Multi-agent workflow (conceptual)

If you are acting as or simulating multiple roles/agents, they should roughly map to:

- **Orchestrator** — decides which steps/agents are needed.
- **Implementation Planner** — breaks work into steps aligned with architecture.
- **Backend Implementer** — writes backend code following the rules above.
- **Frontend Implementer** — writes frontend code following the rules above.
- **Architecture Reviewer** — checks for Clean Architecture, CQRS, layering, and boundaries.
- **Code Quality Reviewer** — checks readability, naming, duplication, and style.
- **Test Writer** — writes unit/integration/e2e tests.
- **Documentation Agent** — updates docs in `/docs` when behavior or architecture changes.

Each role should stay within its scope and not assume responsibilities of others.

---

## 12. Template philosophy

This repository is a **template**, not a finished product.

You must:

- Keep all naming and logic **generic**.
- Avoid domain-specific assumptions.
- Avoid hard-coding product-specific behavior.
- Prefer patterns and structures that can be reused across many apps.

When this template is forked, app-specific features and behavior will be added in the fork, not here.

---

## 13. Agent system

This repository uses a multi-agent system. Specialist agent prompt files live in `.github/prompts/` and are available as selectable agents in VS Code Copilot Chat.

| Agent file | Purpose |
|---|---|
| `orchestrator.prompt.md` | Plans tasks and routes to specialist agents — start here for any non-trivial request |
| `backend-implementer.prompt.md` | .NET 10 · Minimal APIs · CQRS · Clean Architecture · Cosmos DB · native validation |
| `frontend-implementer.prompt.md` | Angular 21 · standalone components · Signals · SCSS |
| `architecture-reviewer.prompt.md` | Reviews backend for Clean Architecture compliance, CQRS correctness, layer violations |
| `code-reviewer.prompt.md` | Reviews naming, readability, dead code, and style |
| `infrastructure-agent.prompt.md` | Bicep IaC · Azure resources · multi-environment params |
| `test-writer.prompt.md` | xUnit + NSubstitute (backend) · Vitest + Playwright (frontend) |
| `docs-agent.prompt.md` | Keeps `/docs` accurate and up to date |

**When working in general chat (not a specialist agent):**

- Before writing any code, identify which agent's rules apply and follow them.
- Read the relevant `.github/prompts/*.prompt.md` file if you need to verify the rules.
- Never mix agent scopes — backend rules do not apply to frontend work and vice versa.
- Validation rule: use `.NET 10 native validation` (`[Required]`, `[StringLength]`, `IValidatableObject`) — **FluentValidation is banned**.

---

## 14. How to respond as Copilot

When generating or modifying code:

1. Respect the folder boundaries (`backend/`, `frontend/`, `infrastructure/`, `docs/`).
2. Follow the architecture rules (Clean Architecture, CQRS, Minimal APIs, Angular 21, Signals, Cosmos, Bicep).
3. Keep the implementation generic and reusable.
4. When appropriate, describe:
   - Where files should go
   - How they interact
   - How they align with the architecture
5. When asked for docs, prefer updating or generating content consistent with `/docs`.
6. When in doubt about rules, read the appropriate agent file in `.github/prompts/` before proceeding.

This file, together with the docs in `/docs` and the agents in `.github/prompts/`, defines how you should behave inside this repository.
