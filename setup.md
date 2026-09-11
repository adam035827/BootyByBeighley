# Agent Container Setup

## Purpose

This project uses a Docker-based development environment for running Claude Code against the Booty by Beighley Target Codebase. The environment provides a reproducible .NET and Node.js toolchain while limiting host filesystem access to this repository.

## Build the Image

From the repository root, run:

```powershell
docker compose build course-environment
```

This builds the local image:

```text
bootybybeighley-course-environment:latest
```

The image includes:

- .NET SDK 10
- Node.js 22 and npm
- Claude Code
- OpenCode
- ngrok
- Git, Bash, curl, CA certificates, nano, and procps

PostgreSQL is not installed in the agent image. It runs as a separate `postgres:15-alpine` service through Docker Compose. Angular and other frontend packages are installed from `frontend/app/package-lock.json` when needed.

## Run the Agent Container

From the repository root, run:

```powershell
docker compose run --rm course-environment
```

Then launch Claude Code inside the container:

```bash
claude
```

A successful container shell uses `/workspace` as its working directory and displays a prompt similar to:

```text
ai-course:/workspace#
```

## Mounted Paths and Persistence

The runtime uses two intentional persistent mounts:

| Container path | Type | Purpose |
|---|---|---|
| `/workspace` | Read-write bind mount | Exposes only the Booty by Beighley repository and persists project changes to the host |
| `/claude-auth` | Docker named volume | Persists only the Claude Code credential file between disposable containers |

Container-local paths such as `/tmp` and most of `/root` are ephemeral. Claude transcripts, preferences, caches, scratch files, and other container-local changes disappear when the `--rm` container exits. Intended project output must be written under `/workspace`.

The host home directory, SSH directory, cloud configuration, parent folders, and unrelated repositories are not mounted.

## Network Mode

Normal Claude sessions use the default Docker Compose network. Network access is required for:

- Claude authentication and model requests
- NuGet and npm package downloads
- Communication with the PostgreSQL Compose service
- Project access to external Azure services when configured

Complete network isolation is tested separately:

```powershell
docker run -it --rm --network none -v "${PWD}:/workspace" bootybybeighley-course-environment:latest
```

Inside the isolated container:

```bash
curl https://example.com
```

The expected result is a DNS or network error. This verifies that outbound access can be blocked, but `--network none` is not suitable for normal Claude sessions.

## Claude Authentication

The first Claude login writes a credential inside the container. The course entrypoint copies that credential to `/claude-auth` within a few seconds. After the first successful login, wait at least six seconds before exiting.

A fresh container should restore the credential automatically and open Claude without another login. Login URLs, verification codes, and credential files must never be shared, exported, or committed.

## Smoke Test

The following prompt was submitted to Claude Code inside the constrained container:

```text
List the files in /workspace and write a short summary of what this repo does to /workspace/summary.txt. Do not access or modify anything outside /workspace.
```

Relevant terminal output:

```text
Container bootybybeighley-postgres-1 Healthy
Container bootybybeighley-course-environment-run-1440e8859e37 Created

Claude Code v2.1.269
Sonnet 5 · Claude Team
/workspace
```

Before creating the file, Claude displayed this permission prompt:

```text
Do you want to create summary.txt?
1. Yes
2. Yes, and switch to accept edits
3. No
```

Only option 1 was approved. After Claude completed the task and the disposable container exited, `summary.txt` was present in the host repository. This proved that:

- Claude authenticated and ran inside the container.
- Claude could read the repository mounted at `/workspace`.
- Claude requested permission before writing the file.
- Output written under `/workspace` persisted on the host.
- The agent did not require access to the host home or SSH directories.
- Agent output still requires human review; the generated summary contained an inaccurate database detail.

## Security Decisions

### What remains ephemeral?

Temporary files, caches, Claude transcripts and preferences, scratch data, and other changes under `/tmp`, `/root`, or the container writable layer remain ephemeral. They are removed with the disposable container.

### What persists?

The Booty by Beighley repository persists through the `/workspace` bind mount. Claude authentication persists separately through the `/claude-auth` named volume.

### What dependencies were included?

The image includes the .NET 10 SDK, Node.js 22 with npm, Claude Code, OpenCode, ngrok, and the Linux utilities retained from the LaunchCode Dockerfile. Project NuGet and npm packages are restored from repository manifests when needed. PostgreSQL remains a separate Compose service.

### What risks remain?

The agent has read-write access to the entire mounted repository and could modify or delete project files. Normal Claude sessions require network access, which creates a possible data-exposure path. The Docker credential volume is not encrypted or a secrets manager, and any container permitted to mount it may be able to use the stored login. Development credentials must not be reused in production. Agent conclusions and edits may also be inaccurate.

These risks are reduced by mounting only the Target Codebase, keeping host credentials unmounted, using non-production configuration, approving edits deliberately, inspecting Git changes, running tests, and reviewing all generated output before committing it.

## Baseline Verification

The adapted image was built cleanly and the installed tools were verified:

```text
.NET SDK:    10.0.401
Node.js:     v22.23.2
npm:         10.9.8
Claude Code: 2.1.269
```

The filesystem test confirmed that `/workspace` contains the selected repository, changes there persist on the host, and unmounted host paths are unavailable. This setup is the provisional baseline for later parallel-agent and orchestration exercises and should be expanded only when a demonstrated project requirement needs additional tools, mounts, credentials, persistence, or network access.
