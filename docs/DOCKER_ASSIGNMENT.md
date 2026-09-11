# Docker Containerization Assignment

## Purpose

This repository is an existing application being used for an AI course assignment. The assignment is to:

1. Copy the Dockerfile supplied by the course demo.
2. Repurpose that Dockerfile for the Booty by Beighley codebase.
3. Containerize the existing application without redesigning the application itself.
4. Document and verify the dependencies needed inside the container.

The current Docker work is an adaptation exercise. The course Dockerfile is the starting point, but its Python-based environment and course tooling do not automatically satisfy this application's runtime requirements.

The course is delivered through LaunchCode. The Claude Code setup supplied in the demo Dockerfile is part of the required learning environment and must remain available after the Dockerfile is adapted to this repository.

The course's initial image and repository-mount exercise was completed before this project-specific assignment. This document builds on that work by adapting the same sandboxing pattern to Booty by Beighley.

### Course repositories

Course work occurs in two distinct repositories:

1. The **Sandbox Example** is the forked LaunchCode repository used to practice lesson patterns in a low-risk environment.
2. The **Target Codebase** is the real project used for Capstone exercises. Booty by Beighley is this student's Target Codebase.

Each activity must be checked before editing so changes are made in the intended repository. This Dockerfile and this assignment document belong to the Target Codebase, not the original LaunchCode course repository.

The intended result is a reproducible agent environment: each student or team member can build the same image, receive the same project tools, and run with the same filesystem and credential boundaries. Project dependencies belong in the Dockerfile at image-build time rather than being installed manually in individual running containers.

Minimalism is a design requirement. The image should contain the LaunchCode agent harness and only the tools needed to inspect, modify, build, test, and document this codebase. A focused image is easier to reproduce, audit, troubleshoot, and run in multiple simultaneous agent sessions without unexpected differences.

### Threat model

A coding agent can use the filesystem and network permissions available to the process that launched it. Running an agent directly on a developer workstation could expose SSH keys, browser sessions, cloud credentials, API keys, environment files, customer data, and unrelated private repositories.

Sandboxing reduces that attack surface. It does not guarantee that an agent will never make a mistake, but it limits what mistakes, hallucinated commands, prompt injection, or unsafe tool use can reach. This environment therefore follows least privilege:

- Mount only the selected Target Codebase.
- Do not mount the host home directory or credential directories.
- Persist only the project workspace and the dedicated Claude authentication file.
- Add only tools and network access required for the current work.
- Review agent changes before retaining or committing them.

These protections must be observable and measurable rather than assumed from the Dockerfile or Compose configuration. The filesystem, persistence, credential-restoration, network-isolation, and agent smoke tests in this document provide repeatable evidence that the intended boundaries hold.

LaunchCode also provides the original pre-built course image at:

```text
us-central1-docker.pkg.dev/hire-human/hire-human-ai/agentic_engineer_1:latest
```

That registry image is the course baseline containing Claude Code and a minimal Linux environment with no credentials baked in. This repository uses its own adapted Dockerfile and local image, `bootybybeighley-course-environment:latest`, so the project-specific .NET and Node.js requirements are explicit and reproducible.

The starter Dockerfile came from the `module_1` Sandbox Example in the LaunchCode course repository:

```text
https://github.com/LaunchCodeEducation/LaunchCodeAgenticEngineer
```

Forking and cloning that course repository was part of the earlier course setup. The original course Dockerfile remains in the course repository; this project contains a separate copy that can be adapted without changing the source material.

## Image and Container Lifecycle

- A Docker image is a read-only, reusable blueprint.
- A container is a running instance of an image.
- Changes written only to a container's writable layer disappear when that container is removed.
- Rebuilding an image does not include changes made interactively inside an earlier container.
- The `--rm` option automatically removes a container after its command exits, making it useful for throwaway checks.
- Bind mounts and named volumes have separate lifecycles and can persist after a container is removed.
- No real credentials should be copied into the image or committed to the repository.

For this setup, the image contains tools and configuration, the `/workspace` bind mount contains project source, and the `claude-auth` named volume contains Claude Code authentication state.

### Building the adapted image

The course's generic example builds an image named `agentic_engineer_1`. This project uses a descriptive local name instead:

```powershell
docker build -t bootybybeighley-course-environment:latest .
```

The equivalent Compose command is:

```powershell
docker compose build course-environment
```

The first build downloads and installs the base image and dependencies and may take several minutes. Later builds normally reuse unchanged Docker layers. Use `--no-cache` only when a deliberately clean rebuild is needed; it is not required for routine development.

Confirm that the adapted image exists locally with:

```powershell
docker images
```

The output should include `bootybybeighley-course-environment` with the `latest` tag.

### Inspecting the isolated image

Before adding a bind mount, a throwaway container can show the image's own filesystem:

```powershell
docker run --rm bootybybeighley-course-environment:latest ls /
```

The output should contain normal Linux directories such as `/bin`, `/etc`, `/tmp`, and `/workspace`. It should not contain the Windows home directory or repository contents because host files are invisible until they are explicitly mounted.

This check is intentionally different from `docker compose run --rm course-environment`, because Compose applies the configured repository bind mount and named credential volume. The direct `docker run` command above inspects only the image filesystem.

## Current Application

Booty by Beighley currently contains:

- A .NET 10 Minimal API backend.
- PostgreSQL persistence through EF Core and Npgsql.
- An Angular 21 frontend.
- An NSwag-generated TypeScript API client.
- Development startup behavior that applies EF Core migrations and seeds test data.

The current Compose work includes a PostgreSQL service and a `course-environment` service. Its image is explicitly named `bootybybeighley-course-environment` because it contains the LaunchCode tools and project toolchains rather than a built copy of the application.

The local repository is bind-mounted at `/workspace`. The source code is intentionally not copied into the image: edits made by an agent inside `/workspace` persist to this repository on the host, while files elsewhere in the container remain ephemeral. No home, Desktop, Downloads, SSH, cloud-credential, or other broad host directory is mounted.

## Verified Dependencies

No Docker dependency list was found in the current persistent or session memory. The dependencies below were verified from the repository documentation and project manifests.

### Required for the backend container

| Dependency | Version | Why it is needed | Verified from |
|---|---:|---|---|
| .NET SDK | 10 | Builds and runs the API with `dotnet run`; the project targets `net10.0` | `BootyByBeighley.Api.csproj` and `BootyByBeighley.Infrastructure.csproj` |
| ASP.NET Core runtime | 10 | Runs a published API; already included in the .NET SDK image | `Microsoft.NET.Sdk.Web` in `BootyByBeighley.Api.csproj` |
| CA certificates | Distribution package | Allows trusted HTTPS access during restore and external calls | Course Dockerfile and normal SDK/container requirements |

The SDK is required for the current `dotnet run` command. A later multi-stage production image could build with the SDK and run from the smaller ASP.NET Core 10 runtime image.

### Backend packages restored by NuGet

These are declared in the project files and restored by `dotnet restore`. They do not need separate `apt` installation.

| Package | Version | Purpose |
|---|---:|---|
| `Microsoft.AspNetCore.Authentication.JwtBearer` | 10.0.5 | JWT bearer authentication |
| `Microsoft.AspNetCore.OpenApi` | 10.0.1 | OpenAPI document support |
| `Microsoft.Extensions.ApiDescription.Server` | 10.0.1 | API description generation at build time |
| `Microsoft.EntityFrameworkCore` | 10.0.0 | ORM and database migrations |
| `Microsoft.EntityFrameworkCore.Design` | 10.0.0 | EF Core design-time and migration tooling support |
| `Npgsql.EntityFrameworkCore.PostgreSQL` | 10.0.0 | PostgreSQL EF Core provider |
| `Azure.Storage.Blobs` | 12.24.1 | Movement-video blob storage |
| `Azure.Identity` | 1.14.1 | Azure credential discovery |
| `Microsoft.Extensions.Configuration.Abstractions` | 10.0.5 | Infrastructure configuration abstractions |
| `Microsoft.Extensions.DependencyInjection.Abstractions` | 10.0.5 | Application and infrastructure DI abstractions |
| `Microsoft.Extensions.Hosting.Abstractions` | 10.0.5 | Infrastructure hosting abstractions |
| `Newtonsoft.Json` | 13.0.4 | JSON handling in infrastructure |

### Separate services and external resources

| Dependency | Version or endpoint | Container requirement |
|---|---|---|
| PostgreSQL | 15 Alpine currently in Compose; repository docs specify PostgreSQL 16 | Run as a separate Compose service, expose port `5432`, and provide a health check |
| Azure Blob Storage | Configured container URI | The API requires `Azure:BlobStorage:ContainerUri` during startup; actual blob operations also require credentials accepted by `DefaultAzureCredential` |
| Azure AD B2C / JWT issuer | External service | Required for real authenticated flows; no local identity-provider container is currently defined |

PostgreSQL should remain a separate service rather than being installed in the application image. The PostgreSQL 15 versus 16 mismatch must be resolved before the final Compose configuration is chosen.

### Required runtime configuration

| Setting | Current Docker value or source | Purpose |
|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Development` | Enables migrations, seed data, and OpenAPI mapping |
| `ConnectionStrings__DefaultConnection` | Compose connection string using host `postgres` | Connects the backend container to the PostgreSQL service |
| `Azure__BlobStorage__ContainerUri` or equivalent JSON setting | Currently supplied by `appsettings.Development.json` | Required when infrastructure services are registered |
| Azure credential environment variables | Not currently defined | Needed only when blob operations must authenticate from the container |
| HTTP listen URL | Launch profile currently uses `http://localhost:5118`; Docker must override it to `http://0.0.0.0:5118` | Binding only to localhost would make the API unreachable through the published port |

The API is expected on host port `5118`, PostgreSQL on `5432`, and the Angular development server on `4200` if the frontend is containerized.

### Required only if the container builds or runs the frontend

| Dependency | Version | Why it is needed |
|---|---:|---|
| Node.js | 22 LTS | Angular build and development tooling |
| npm | 11.1.0 package-manager declaration | Installs frontend packages and runs scripts |
| Angular CLI | 21 | Builds or serves the Angular application |
| NSwag CLI | Latest/current project tool | Regenerates the TypeScript API client from the backend OpenAPI document |

### Frontend packages restored by npm

These packages are installed with `npm ci` from `frontend/app/package-lock.json`; they should not be installed individually as operating-system packages.

| Package group | Version | Purpose |
|---|---:|---|
| Angular common, compiler, core, forms, platform-browser, and router | 21.2.x | Frontend framework and browser application |
| RxJS | 7.8.x | Reactive streams used by Angular |
| `tslib` | 2.3.x | TypeScript runtime helpers |
| Angular build, CLI, and compiler CLI | 21.2.x | Compile and serve the frontend |
| TypeScript | 5.9.x | Frontend compilation |
| Vitest | 4.0.8 | Unit tests |
| jsdom | 28.0.0 | Browser-like unit-test environment |
| Playwright | 1.59.1 | End-to-end browser tests |
| Prettier | 3.8.1 | Formatting during development |

Playwright additionally requires downloaded browser binaries and their Linux system libraries when end-to-end tests run inside a container. Those large dependencies are unnecessary for a normal application image.

### Test-only backend packages

These are restored only when backend tests are built or run in the container:

| Package | Version |
|---|---:|
| `Microsoft.NET.Test.Sdk` | 17.14.1 |
| xUnit | 2.9.3 |
| xUnit Visual Studio runner | 3.1.4 |
| NSubstitute | 5.3.0 |
| Coverlet collector | 6.0.4 |

### Required LaunchCode course environment

The copied course Dockerfile currently installs the following tools:

- Node.js and npm, which are required to install and run Claude Code.
- Claude Code through the `@anthropic-ai/claude-code` npm package.
- The LaunchCode-supplied Claude settings from `settings.json`.
- The course status line from `statusline.sh`.
- The `docker-entrypoint.sh` authentication workflow, including the `/claude-auth` credential location and onboarding state.
- An interactive shell environment with Bash, Git, CA certificates, and the course shell customizations.

These components must be preserved while adding the .NET and application dependencies. The entrypoint must continue to end with `exec "$@"` so it can launch either an interactive course shell or the application command correctly.

### Demo tools requiring course confirmation

The copied Dockerfile also contains items whose necessity is not yet confirmed:

- Python 3.12 as the base image.
- OpenCode.
- ngrok.
- `curl`, where it is not needed by another installation step.
- `nano` and `procps` convenience utilities.

These may still be useful or required by later LaunchCode lessons, but they are not dependencies of the Booty by Beighley application itself. They should not be removed until the course instructions are checked.

### Installation ownership

To avoid installing the same dependency in multiple ways:

| Dependency type | Installation mechanism |
|---|---|
| .NET SDK or ASP.NET runtime | Docker base image |
| Backend NuGet packages | `dotnet restore` |
| Node.js and npm | Node base image or an explicitly configured Node.js repository |
| Frontend JavaScript packages | `npm ci` in `frontend/app` |
| PostgreSQL | Separate official PostgreSQL Compose image |
| Playwright browsers | `npx playwright install --with-deps` only in a test image |
| Claude Code and LaunchCode configuration | Dockerfile copy/install steps and the existing entrypoint |
| Other course command-line tools | Dockerfile installation steps, pending confirmation from the course instructions |

Project-wide runtimes and global command-line tools belong in the image at build time, not in ad hoc commands run after each container starts. This ensures every new container begins with the same capabilities. Repository-declared packages are handled by their normal package managers from the mounted source: `dotnet restore` reads the project files, and `npm ci` reads the frontend lockfile.

The relevant Dockerfile instructions are:

- `FROM` selects the base image. This project uses the .NET 10 SDK image because .NET is the primary project runtime.
- `RUN` executes installation or configuration during `docker build` and saves the result as an image layer. Related dependencies may be grouped into one `RUN` instruction.
- `docker build -t ...` creates and tags a reusable image whose installed tools are available to every later container.

The project-specific image tag is `bootybybeighley-course-environment:latest`. It replaces the generic course image name in runtime commands while preserving the same sandbox workflow.

### Filesystem boundary

The `/workspace` mount is a deliberate trust boundary. A coding agent needs access to the selected repository to inspect code, edit files, generate tests, run project commands, and write documentation, but it does not need unrestricted access to the rest of the host machine. The container begins with an isolated filesystem; host content becomes visible only through an explicit mount.

The underlying Docker bind-mount syntax is:

```text
-v /host/path:/container/path:mode
```

- The first path is the selected host directory.
- The second path is where that directory appears in the container.
- `rw` allows reads and writes and is the default.
- `ro` permits reads but prevents container writes.

Compose expresses the same relationship with the `volumes` entry `.:/workspace`. This project requires read-write access because the coding agent must modify source and produce course artifacts that persist on the host.

| Path | Behavior |
|---|---|
| Host repository root | The smallest practical mount because the agent needs the backend, frontend, tests, configuration, and documentation |
| `/workspace` | Bind-mounted project directory; intended source changes persist on the host |
| `/claude-auth` | Named Docker volume containing only the Claude Code credential persisted by the course entrypoint |
| `/tmp` and other container paths | Ephemeral; discarded with the container |
| Host home and credential directories | Not mounted and must remain inaccessible |

The Compose service is configured with an interactive terminal and uses the Dockerfile's Bash command. The backend can be started explicitly from inside the sandbox when needed; it is not the container's default command.

The boundary grants the agent access to exactly one host folder: the Booty by Beighley repository. It does not grant access to the parent directory, other repositories, the Windows user profile, or unrelated host files.

Inside the running container, verify both sides of the boundary:

```bash
ls /home
ls /workspace
```

`/home` must not contain the Windows host account's files. `/workspace` should contain the Booty by Beighley repository because that is the one host directory explicitly mounted for the agent.

The project bind mount and Claude named volume serve different purposes:

- The `/workspace` bind mount persists project work and exposes the selected host repository.
- The `claude-auth` named volume persists only the Claude credential file managed by the course entrypoint.
- The named volume does not replace `/root/.claude`; course settings and tools remain supplied by the image.

The runtime therefore has three storage categories:

1. Container-local storage, including `/tmp` and most of `/root`, is deleted when a `--rm` container exits.
2. `/workspace` is backed by the selected host repository, so source changes and project deliverables persist and appear immediately on the host.
3. `/claude-auth` is a Docker-managed named volume used only to restore and save the Claude credential file.

The authentication volume does not persist Claude transcripts, runtime changes, arbitrary settings changes, or other files under `/root/.claude`. Image-baked settings and skills return to the state defined by the image whenever a new container starts. Any intended course deliverable belongs under `/workspace`.

A Docker volume is neither encryption nor a secrets manager. Any container granted access to `claude-auth` may be able to use its stored login. The volume must not be exported, shared, copied into the image, or committed to source control.

### Starting the scoped environment

The course's raw Docker pattern is:

```powershell
docker run -it --rm -v "${PWD}:/workspace" -v "claude-auth:/claude-auth" agentic_engineer_1
```

For this repository, Compose records the same mount and terminal settings so they do not have to be repeated manually:

```powershell
docker compose run --rm course-environment
```

This starts the adapted `bootybybeighley-course-environment:latest` image with:

- An interactive terminal through `stdin_open: true` and `tty: true`.
- The current repository mounted read-write at `/workspace`.
- A Docker-managed Claude credential volume mounted at `/claude-auth`.
- Automatic removal of the temporary course container after exit.

Compose creates its declared named volume automatically if it does not exist and reuses it on later runs. Therefore, the separate course command `docker volume create claude-auth` is unnecessary for the Compose workflow. The actual Compose-managed volume can be identified with `docker volume ls` and examined with `docker volume inspect <volume-name>` without displaying or exporting its credential contents.

Never replace the repository mount with a broad mount such as `-v ~:/home/user`. Mounting the entire host home directory would expose SSH keys, environment files, browser data, cloud credentials, and unrelated projects, defeating the sandbox boundary.

### Persistence test

From inside a disposable course container:

```bash
echo "This is a test!" > /tmp/test.txt
echo "This is a test!" > /workspace/test.txt
exit
```

After exit, the host repository should contain `test.txt` from `/workspace`. The file under `/tmp` disappears with the removed container. Remove the host test file after verifying the behavior if it is not an intended project artifact.

### Network boundary test

A separate throwaway container can verify complete network isolation without changing the normal Compose configuration:

```powershell
docker run -it --rm --network none -v "${PWD}:/workspace" bootybybeighley-course-environment:latest
```

Inside that container, attempt an external request:

```bash
curl https://example.com
```

The expected result is a DNS or network error such as `Could not resolve host` or `Network is unreachable`. This confirms that `--network none` blocks all egress, preventing the container from calling external APIs or sending repository data over the network.

This is a boundary-verification test, not the normal development mode. With networking disabled:

- Claude Code cannot authenticate or access its model service.
- NuGet and npm cannot download packages.
- The container cannot reach PostgreSQL over the Compose network.
- External Azure services are unavailable.

The regular Compose environment therefore needs network access for useful agent work and local service communication. Future course work may replace broad network access with narrower, auditable capabilities such as approved MCP servers. Network access should be granted intentionally according to the task rather than assumed to be unrestricted or permanently disabled.

### Network-enabled Claude smoke test

After verifying filesystem isolation and no-network behavior separately, launch the normal course environment with networking enabled:

```powershell
docker compose run --rm course-environment
```

This preserves both intended mounts:

- The repository bind mount at `/workspace`.
- The Claude credential volume at `/claude-auth`.

It also permits the outbound access Claude Code needs for authentication and model requests. Do not add `--network none` for this smoke test.

A successful launch should open an interactive shell with a prompt similar to:

```text
ai-course:/workspace#
```

From that prompt, launch the coding agent:

```bash
claude
```

Claude Code is installed in the adapted image. Its network access does not broaden its filesystem access: the agent still sees only the container filesystem, the selected `/workspace` repository mount, and the dedicated `/claude-auth` volume.

### First Claude login

Claude initially writes its Linux credential under the container's `/root/.claude` directory. The course entrypoint polls for credential changes and copies the credential file into `/claude-auth` for reuse by later containers.

After the first authentication succeeds, wait at least six seconds before exiting the container so one full polling interval can complete. Preferences, transcripts, and other files under `/root/.claude` remain disposable unless a later course activity explicitly saves an intended artifact under `/workspace`.

If authentication fails:

- Confirm that the login URL was opened from the current terminal session's sign-in flow.
- Confirm that the container has network access.
- Restart `claude` and begin again if the URL or verification code expired.
- Never share or record the login URL, verification code, or saved credential contents.

### Verify credential restoration

After waiting at least six seconds and exiting the first authenticated container, start a fresh disposable container:

```powershell
docker compose run --rm course-environment
```

Then run:

```bash
claude
```

The entrypoint should restore the saved credential from `/claude-auth` before the shell starts, so Claude Code should not request another login. This verifies that authentication survives container replacement without exposing host credential directories.

This Compose command is the normal constrained invocation for the rest of the course. It allows the network traffic Claude Code requires and exposes only two intentional host-persistent resources to the container:

- The selected project repository at `/workspace`.
- The Docker-managed Claude credential volume at `/claude-auth`.

The container still has its own Linux filesystem, but no host home directory, SSH directory, cloud configuration directory, parent folder, or unrelated repository is mounted into it.

### Agent smoke-test purpose

A smoke test is the first minimal check that the coding agent can perform useful work without a catastrophic setup failure. It should be intentionally small, easy to inspect, and constrained to `/workspace`. Its purpose is to verify the agent, mount persistence, and sandbox boundaries before asking the agent to make substantial project changes.

Claude Code may automatically delegate parts of a task to its built-in subagents:

- **Explore** performs fast, read-only codebase search and analysis.
- **Plan** gathers context and presents implementation strategies without editing.
- **General-purpose** investigates and acts on broader multi-step tasks.

Each subagent uses a separate context and returns a summary to the parent session. These are built into Claude Code; custom agents are introduced later in the course.

### Agent smoke-test task

From the Claude Code session inside the container, use a small prompt that requires no credentials or APIs beyond Claude itself:

```text
List the files in /workspace and write a short summary of what this repo does to /workspace/summary.txt. Do not access or modify anything outside /workspace.
```

Expected behavior:

- Claude reads project files under `/workspace`.
- Claude does not attempt to inspect paths outside `/workspace`.
- Claude creates `/workspace/summary.txt`.
- After the disposable container exits, `summary.txt` is visible in the host repository.

Inspect the generated file before keeping or committing it. The smoke test proves that the mount is readable, authentication works, the agent can write output, and bind-mounted output persists.

### Negative host-access test

The course also tests a path that would exist only on the host. On this Windows machine, ask Claude to inspect a host path that was not mounted, for example:

```text
List the files in C:\Users\adam0. Do not create or modify any files.
```

That Windows host path should not be accessible from the Linux container. This demonstrates that mounting the repository at `/workspace` does not expose the rest of the host filesystem. The container can still access its own Linux directories; the security claim is specifically that unmounted host directories are unavailable.

### Verify smoke-test output on the host

Exit Claude Code and then exit the disposable container:

```text
exit
```

Back in PowerShell at the repository root, confirm that the bind-mounted output exists and inspect it:

```powershell
Test-Path .\summary.txt
Get-Content .\summary.txt
git status --short
```

`Test-Path` should return `True`. The container has been removed, but `summary.txt` remains because `/workspace` is a bind mount backed by the host repository. `git status --short` provides a focused view of files changed inside the project during the smoke test.

Finally, verify that no unexpected files appeared in sensitive or unrelated host locations. The scoped mount gives the container no mechanism to write to unmounted host paths, but checking the host after the first agent run confirms the boundary in practice. Do not expose or inspect secret contents during this verification; check only for unexpected changes.

This completes the smoke-test loop: the agent ran inside an isolated container, used only the selected repository and credential mounts, produced a reviewable project artifact, and left that artifact accessible on the host.

### Build and startup commands implied by the repository

| Task | Command |
|---|---|
| Restore backend packages | `dotnet restore backend/BootyByBeighley.sln` |
| Build backend | `dotnet build backend/BootyByBeighley.sln` |
| Run backend in the current development setup | `dotnet run --project backend/src/BootyByBeighley.Api/BootyByBeighley.Api.csproj` |
| Run backend tests | `dotnet test backend/BootyByBeighley.sln` |
| Install frontend packages reproducibly | `npm ci --prefix frontend/app` |
| Build frontend | `npm run build --prefix frontend/app` |
| Run frontend development server | `npm start --prefix frontend/app -- --host 0.0.0.0` |
| Regenerate API client while backend is running | `nswag run nswag.json` |

NSwag is a development workflow dependency, not an API runtime dependency. The checked-in generated client allows normal frontend builds without running NSwag unless the backend contract changed.

## Existing Startup Behavior

When the API runs with `ASPNETCORE_ENVIRONMENT=Development`, `Program.cs`:

1. Connects to PostgreSQL using `ConnectionStrings__DefaultConnection`.
2. Applies pending EF Core migrations.
3. Seeds development data.
4. Exposes the OpenAPI document.

The container therefore needs network access to the PostgreSQL Compose service and must not start the API until PostgreSQL is healthy.

## Verified Baseline

The initial course Dockerfile used `python:3.12-slim`, which did not provide the .NET SDK and installed an unsupported Node.js 20 package. The adapted image now uses the .NET 10 SDK base and installs Node.js 22 explicitly.

A clean build and direct image check verified:

| Component | Verified version |
|---|---:|
| .NET SDK | 10.0.401 |
| Node.js | 22.23.2 |
| npm | 10.9.8 |
| Claude Code | 2.1.269 |

The filesystem smoke test also verified that:

- The repository appears at `/workspace`.
- A file written under `/workspace` persists on the host.
- `/host` is not mounted.
- The host SSH directory is not exposed at `/root/.ssh`.
- PostgreSQL starts and passes its health check.
- The disposable course container can be removed without removing bind-mounted project files.

Together, the direct image inspection and Compose smoke test demonstrate the intended boundary: the image starts isolated, and only the explicitly configured repository and Claude credential volume become visible at runtime.

Claude credentials are intentionally stored in the `claude-auth` named volume rather than in the image or project mount. This allows disposable course containers to reuse authentication without exposing the host home directory.

## Decisions to Make Before Editing

1. Which explicit commands should students use inside the interactive course environment to start and test each application component?
2. Must Docker run only the backend and PostgreSQL, or the Angular frontend as well?
3. Besides Claude Code and its configuration, are Python, OpenCode, and ngrok mandatory for later LaunchCode lessons?
4. Should the backend use `dotnet run` for development, or should Docker build and run a published application artifact?
5. Should PostgreSQL use version 15 from the current Compose file or version 16 from the repository prerequisites?

## Initial Success Criteria

For the current backend and database scope:

- The image builds from the repository root.
- The image contains a working .NET 10 SDK or runtime appropriate to its startup command.
- The backend starts through Docker Compose.
- PostgreSQL reports healthy before backend startup.
- The backend connects to `postgres:5432`.
- Development migrations and seed data complete successfully.
- The API listens on the configured container port and is published to host port 5118.
- An API endpoint responds successfully from the host.

## Reusable Environment Checklist

When a later course exercise or parallel agent session fails, return to this baseline:

1. Confirm Docker Desktop is running with `docker info`.
2. Confirm `bootybybeighley-course-environment:latest` exists with `docker images`.
3. Rebuild the image after Dockerfile dependency changes.
4. Confirm the container opens at `/workspace`.
5. Confirm only the selected repository is mounted from the host.
6. Confirm .NET, Node.js, npm, and Claude Code report the expected versions.
7. Confirm the `claude-auth` volume restores authentication without exposing host credentials.
8. Confirm required network access is enabled for Claude and disabled only during explicit isolation tests.
9. Confirm PostgreSQL is healthy before running database-dependent commands.
10. Confirm intended outputs are written under `/workspace` so they persist and can be reviewed on the host.

This image and runtime configuration form the baseline for later parallel-agent and orchestration exercises. Each agent session should receive a predictable runtime and an intentionally selected workspace; parallel sessions must not gain broader host access merely for convenience.

### Parallel agent model

The image does not need special awareness of Git worktrees or parallel sessions. Each container receives one selected host directory at the same internal path, `/workspace`.

For example, one agent container may mount a host worktree for `feature-a`, while another mounts a separate worktree for `feature-b`. Inside both containers, the agent simply works in `/workspace`. The different host mounts keep their source files and changes separate even though both sessions use the same image and internal path.

This gives parallel sessions:

- The same reproducible toolchain from the shared image.
- Separate container processes and writable layers.
- Separate project files when each session mounts a different host worktree.
- A consistent `/workspace` path that requires no image changes.

Filesystem isolation between agents depends on mounting distinct host directories. Starting multiple containers against the same writable host directory would allow them to modify the same files and is not an isolated parallel workflow.

### Using an agent during setup

Claude Code or another coding agent should be launched from inside the course container rather than directly on the host. It may inspect the mounted repository, identify install/build/test/start commands, and help diagnose pasted errors while retaining the configured filesystem boundary.

The agent's suggestions and edits still require human review. Only intentional changes under `/workspace` should be retained, and new tools should be added to the Dockerfile only when the project or course demonstrably requires them.

## Course Setup Checks

Before working in the agent container:

```powershell
docker --version
docker info
```

`docker --version` confirms that the CLI is installed. `docker info` confirms that Docker Desktop and the Docker engine are actually running. The course's written commands take precedence over older commands shown in its videos.

## Relevant Files

- `Dockerfile` - copied course image being repurposed
- `docker-entrypoint.sh` - course authentication and startup wrapper
- `docker-compose.yml` - PostgreSQL and LaunchCode course-environment orchestration
- `.dockerignore` - Docker build-context exclusions
- `backend/src/BootyByBeighley.Api/BootyByBeighley.Api.csproj` - backend target framework and packages
- `backend/src/BootyByBeighley.Api/Program.cs` - migrations, seeding, and API startup
- `backend/src/BootyByBeighley.Infrastructure/BootyByBeighley.Infrastructure.csproj` - database and infrastructure packages
- `frontend/app/package.json` - frontend tool and package versions
- `docs/INSTRUCTIONS.md` - repository prerequisites and stack versions
- `DOCKER_FINDINGS.md` - prior experiments and hypotheses; findings should be revalidated against the current files

## Working Approach

Make one small, explained Docker change at a time. After each change, run the narrowest check that can confirm or reject the current hypothesis. Do not redesign the application or add unrelated deployment infrastructure as part of this assignment.

## Final Setup Note

This is the baseline invocation for running a coding agent safely against the Booty by Beighley Target Codebase.

### Build

From the repository root:

```powershell
docker compose build course-environment
```

This builds `bootybybeighley-course-environment:latest` with the .NET 10 SDK, Node.js 22, Claude Code, and the retained LaunchCode tools.

### Run

```powershell
docker compose run --rm course-environment
```

Then launch Claude inside the container:

```bash
claude
```

The Compose invocation provides:

- A read-write bind mount from the repository root to `/workspace`.
- A named volume at `/claude-auth` for the Claude credential file.
- An interactive terminal.
- The default Compose network, which permits Claude model access and communication with the PostgreSQL service.
- Automatic removal of the disposable course container after exit.

The normal agent workflow is network-enabled because Claude Code requires outbound access. Complete network isolation is tested separately with `--network none`; it is not used for normal Claude sessions.

### Smoke test

Prompt submitted to Claude Code:

```text
List the files in /workspace and write a short summary of what this repo does to /workspace/summary.txt. Do not access or modify anything outside /workspace.
```

Relevant terminal output from launching the smoke-test container:

```text
Container bootybybeighley-postgres-1 Healthy
Container bootybybeighley-course-environment-run-1440e8859e37 Created

Claude Code v2.1.269
Sonnet 5 · Claude Team
/workspace
```

Claude displayed the proposed contents and asked:

```text
Do you want to create summary.txt?
1. Yes
2. Yes, and switch to accept edits
3. No
```

Only option 1 was approved. After Claude completed the task, the disposable container exited and `/workspace/summary.txt` was present in the host repository. Review found one inaccurate statement about the database, demonstrating that persisted agent output still requires human verification.

### Provisional security choices

- **Ephemeral:** `/tmp`, most of `/root`, Claude transcripts and preferences, caches, scratch files, and other container-local changes.
- **Persistent:** the selected repository at `/workspace` and only the Claude credential file through `/claude-auth`.
- **Dependencies:** .NET 10 SDK, Node.js 22 with npm, Claude Code, OpenCode, ngrok, Git, Bash, curl, CA certificates, nano, and procps. PostgreSQL remains a separate Compose service. Repository NuGet and npm packages are restored from project manifests when needed.
- **Filesystem access:** only the Target Codebase is bind-mounted. Host home, SSH, cloud configuration, parent directories, and unrelated repositories are not mounted.
- **Network access:** normal agent sessions use the Compose network because Claude and project package managers require egress and the backend requires PostgreSQL connectivity. A separate `--network none` test verifies that egress can be fully blocked.
- **Remaining risks:** the agent can change any file in the writable repository, network access creates a possible data-exposure path, the Docker credential volume is not a secrets manager, development credentials must never be reused in production, and generated conclusions or edits may be incorrect.
- **Mitigations:** use the smallest practical mount, keep credentials out of the image and repository, use non-production values, retain manual permission approval, inspect `git status` and diffs, run project tests, and review all agent output before committing it.

These decisions are provisional. Revisit them only when a later course activity or a demonstrated project requirement needs additional tools, mounts, credentials, persistence, or network access.
