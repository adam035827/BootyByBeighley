# Agents

This document is the workflow guide for the multi-agent development system. All agent files live in `.github/agents/`.

## How to use

Select the **Orchestrator** from the VS Code Copilot Chat agent picker, then describe the feature or change you want. The orchestrator will plan the work and automatically invoke the correct specialist subagents in sequence.

Specialist agents are hidden from the picker (`user-invocable: false`) — the orchestrator delegates to them automatically. You never need to invoke them manually.

---

## When to use agents

Use the **Orchestrator** for any non-trivial request. For a simple, single-layer fix (e.g., only changing one backend file) you can invoke a specialist directly from the picker by switching its `user-invocable` to `true` temporarily, or just use the default agent.

---

## Agent Catalog

### Orchestrator
- **File**: `.github/agents/orchestrator.agent.md`
- **Picker**: ✅ Visible
- **Use when**: Starting any new feature, change, or cross-cutting task
- **Scope**: Cross-cutting — invokes all other agents as subagents
- **Does not**: Write implementation code

---

### Backend Implementer
- **File**: `.github/agents/backend-implementer.agent.md`
- **Picker**: Hidden (subagent only)
- **Use when**: Creating or modifying any backend code — commands, queries, endpoints, repositories
- **Scope**: `backend/` only
- **Does not**: Write frontend code, IaC, or tests

---

### Frontend Implementer
- **File**: `.github/agents/frontend-implementer.agent.md`
- **Picker**: Hidden (subagent only)
- **Use when**: Creating or modifying Angular components, routes, or SCSS
- **Scope**: `frontend/` only
- **Does not**: Write backend code, IaC, or tests

---

### Architecture Reviewer
- **File**: `.github/agents/architecture-reviewer.agent.md`
- **Picker**: Hidden (subagent only)
- **Use when**: Reviewing backend code for Clean Architecture compliance, CQRS correctness, layer boundary violations
- **Scope**: `backend/` (read-only review)
- **Does not**: Fix code — reports violations only

---

### Code Reviewer
- **File**: `.github/agents/code-reviewer.agent.md`
- **Picker**: Hidden (subagent only)
- **Use when**: Reviewing any code for naming, readability, dead code, and style
- **Scope**: `backend/` and `frontend/` (read-only review)
- **Does not**: Review architecture (use `architecture-reviewer` for that)

---

### Infrastructure Agent
- **File**: `.github/agents/infrastructure-agent.agent.md`
- **Picker**: Hidden (subagent only)
- **Use when**: Creating or modifying Bicep modules, environment params, or Azure resource definitions
- **Scope**: `infrastructure/` only
- **Does not**: Write application code

---

### Test Writer
- **File**: `.github/agents/test-writer.agent.md`
- **Picker**: Hidden (subagent only)
- **Use when**: Adding unit, integration, or component tests after implementation
- **Scope**: `backend/tests/` and `frontend/` spec files
- **Does not**: Write implementation code

---

### Docs Agent
- **File**: `.github/agents/docs-agent.agent.md`
- **Picker**: Hidden (subagent only)
- **Use when**: Architecture changes, new features added, or agent workflows updated
- **Scope**: `docs/` only
- **Does not**: Write code

---

## Typical workflow

```
User selects Orchestrator → types feature description
  └─ Orchestrator → plans steps, invokes subagents in order
       ├─ backend-implementer  → Domain → Application → Infrastructure → API
       ├─ frontend-implementer → components, routes, NSwag client usage
       ├─ test-writer          → backend unit tests + frontend component/E2E tests
       ├─ architecture-reviewer → validates backend layer boundaries
       ├─ code-reviewer        → validates quality across both layers
       └─ docs-agent           → updates FEATURES.md / INSTRUCTIONS.md
```
