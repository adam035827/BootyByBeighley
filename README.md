# Modern Architecture Template

This repository is a clean starting point for building modern full‑stack applications. It provides an empty but structured layout for:

- `backend/` — server-side application code  
- `frontend/` — client-side application code  
- `infrastructure/` — Infrastructure-as-Code and deployment assets  
- `docs/` — architecture, features, and workflow documentation  

A `copilot-instructions.md` file is included at the root so VS Code Copilot Chat can understand and follow the project’s architecture and development workflow.

## Purpose

This template is designed to be **generic and reusable**.  
You can fork it to create new applications while keeping a consistent structure and development approach.

## Getting Started

1. Clone or fork this repository.  
2. Open the root folder in VS Code.  
3. Open Copilot Chat and use the bootstrapping prompt below to orient the agent before any scaffolding work.

### Bootstrapping prompt

Paste this into Copilot Chat at the start of every new session in a fork of this template:

```
Load the copilot-instructions.md file and confirm you understand the architecture, folder boundaries, and development workflow. Then tell me what you need from me to begin scaffolding the backend and frontend inside this template.
```

This prompt ensures Copilot has loaded and understood the full architecture before writing any code.

## Running locally

### Prerequisites

| Tool | Version | Notes |
|---|---|---|
| .NET SDK | 10 | `dotnet --version` |
| Node.js | 22 LTS | `node --version` |
| Angular CLI | 21 | `npm install -g @angular/cli` |
| NSwag CLI | Latest | `npm install -g nswag` |
| Cosmos DB Emulator | Latest | [Download](https://aka.ms/cosmosdb-emulator) |

### First-time setup

1. **Install frontend dependencies**
   ```powershell
   cd frontend/app
   npm install
   ```

2. **Configure the Cosmos DB connection string** (stored in .NET user secrets — never committed)
   ```powershell
   cd backend/src/ModernApp.Api
   dotnet user-secrets set "Cosmos:ConnectionString" "<your-connection-string>"
   ```
   For the local emulator, the connection string is:
   ```
   AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b5n6rRMQN/XqXnv9LJGEHTFYdVMPQ==
   ```
   > The emulator key is the publicly documented default — safe to copy.

### Start the application

**Option A — VS Code (recommended)**  
Open the **Run & Debug** panel (`Ctrl+Shift+D`), select **Run Application**, and press **▶**.  
This will, in order:
1. Build the .NET backend
2. Start the backend on `http://localhost:5118` with full debugger support
3. Wait for the backend to be ready, then run `nswag run nswag.json` to regenerate the TypeScript client
4. Start `ng serve` — browser opens automatically at `http://localhost:4200`

**Option B — terminal**
```powershell
# Terminal 1 — backend
cd backend/src/ModernApp.Api
dotnet run

# Terminal 2 — regenerate API client (once backend is up)
nswag run nswag.json

# Terminal 3 — frontend
cd frontend/app
ng serve --host 127.0.0.1
```

### Ports

| Service | URL |
|---|---|
| Backend API | http://localhost:5118 |
| OpenAPI spec | http://localhost:5118/openapi/v1.json |
| Frontend dev server | http://localhost:4200 |
| Cosmos DB Emulator | https://localhost:8081 |

The Angular dev server proxies all `/api/*` and `/openapi/*` requests to the backend — see [`frontend/app/proxy.conf.json`](frontend/app/proxy.conf.json).

## Testing

This template uses a testing pyramid with two tools:

- **Vitest** for unit and component tests (fast, isolated, jsdom-based)
- **Playwright** for end-to-end browser tests (real browser workflows, cross-browser, mobile emulation)

```powershell
# Backend unit tests
cd backend
dotnet test

# Frontend unit/component tests (from frontend/app)
cd frontend/app
npx ng test --watch=false

# E2E tests — requires the backend running; Playwright starts ng serve automatically
cd frontend/app
npx playwright test
```

> **Note:** Playwright connects to `127.0.0.1:4200`. When running `ng serve` manually (not via Playwright's `webServer`), pass `--host 127.0.0.1` so the port binds to IPv4.

## Folder Structure

```
modern-architecture-template/
  backend/         .NET 10, Minimal APIs, Clean Architecture, CQRS
  frontend/        Angular 21, standalone components, Signals, SCSS
  infrastructure/  Bicep IaC targeting Azure
  docs/            Architecture, feature catalog, agent workflow docs
  .github/
    prompts/       Specialist agent prompt files
  copilot-instructions.md  Root instructions file loaded by all agents
```

