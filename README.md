# Booty by Beighley

A mobile-first fitness coaching platform built on a modern, clean architecture.  
Booty by Beighley connects a single coach with her students through a structured workout experience.

The app is distributed via **Apple App Store and Google Play Store** (via Capacitor), with a web-based admin interface for coach content management and a mobile-optimized student experience.

---

## Project Structure

- `backend/` — .NET 10 API with Clean Architecture and CQRS, PostgreSQL database  
- `frontend/` — Angular 21 admin interface (coach) and student mobile app  
- `infrastructure/` — Bicep IaC for Azure deployments  
- `docs/` — Product definition, architecture decisions, feature catalog

## Purpose

This is the Booty by Beighley fitness coaching platform — a mobile-first application that connects a coach with students through structured workouts.

For detailed product requirements, see [docs/APP.md](docs/APP.md).  
For architecture and patterns, see [docs/INSTRUCTIONS.md](docs/INSTRUCTIONS.md).

## Getting Started

1. Clone this repository.  
2. Open the root folder in VS Code.  
3. Follow the **Running locally** section below to set up the development environment.

## Running locally

### Prerequisites

| Tool | Version |
|---|---|
| Git | Latest |
| .NET SDK | 10 |
| Node.js | 22 LTS |
| Angular CLI | 21 (`npm install -g @angular/cli`) |
| NSwag CLI | Latest (`npm install -g nswag`) |
| Docker & Docker Compose | Latest |

### Setup

1. **Clone the repository**
   ```powershell
   git clone https://github.com/adam035827/BootyByBeighley.git
   cd BootyByBeighley
   ```

2. **Install frontend dependencies**
   ```powershell
   cd frontend/app
   npm install
   ```

3. **Configure .NET user secrets**
   ```powershell
   cd backend/src/BootyByBeighley.Api
   dotnet user-secrets init
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Database=booty_by_beighley_dev;User Id=postgres;Password=postgres"
   ```

### Start the application

**Option A — VS Code (recommended)**  
Open **Run & Debug** (`Ctrl+Shift+D`), select **Run Application**, and press **▶**.

**Option B — Terminal**

Terminal 1 — Start PostgreSQL:
```powershell
docker compose up
```

Terminal 2 — Start the backend:
```powershell
cd backend/src/BootyByBeighley.Api
dotnet run
```

Terminal 3 — Regenerate API client (once backend is running):
```powershell
nswag run nswag.json
```

Terminal 4 — Start the frontend:
```powershell
cd frontend/app
ng serve --host 127.0.0.1
```

Open browser at `http://localhost:4200`.

### Ports

| Service | URL |
|---|---|
| Backend API | http://localhost:5118 |
| OpenAPI spec | http://localhost:5118/openapi/v1.json |
| Frontend dev server | http://localhost:4200 |
| PostgreSQL | localhost:5432 (via Docker) |

The Angular dev server proxies all `/api/*` requests to the backend — see [`frontend/app/proxy.conf.json`](frontend/app/proxy.conf.json).

## Testing

This project uses a testing pyramid with two tools:

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
BootyByBeighley/
  backend/
    src/
      BootyByBeighley.Api/        .NET 10 Minimal API, endpoints
      BootyByBeighley.Application/  CQRS commands/queries, business logic
      BootyByBeighley.Domain/       Domain entities, rules, no external deps
      BootyByBeighley.Infrastructure/ EF Core, repositories, Azure storage
    tests/                            Unit & integration tests
  frontend/
    app/                            Angular 21, standalone components, Signals
  infrastructure/                   Bicep IaC for Azure
  docs/                             Architecture, features, product requirements
  docker-compose.yml                PostgreSQL and dev environment
  copilot-instructions.md           Root instructions for all agents
```

