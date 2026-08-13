<div align="center">

# TaskFlow API

> A task management API for teams — workspaces, projects and tasks, built with Clean Architecture on .NET.

<p align="center">

[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-10-512bd4.svg)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-13-239120.svg)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-336791.svg)](https://www.postgresql.org/)
[![Status](https://img.shields.io/badge/status-live-success.svg)](https://taskflow-api-np2z.onrender.com/health)

</p>
</div>

---

## What is TaskFlow?

TaskFlow is a REST API for team task management, in the spirit of a simplified Linear/Trello "behind
the scenes." A user creates a **Workspace**, invites members, organizes work into **Projects**, and
tracks work as **Tasks** with status, priority and an assignee.

It's built as a portfolio piece to demonstrate production-grade backend patterns in .NET: layered
architecture, real authentication/authorization, domain modeling with enforced business rules,
automated testing and CI/CD — not just CRUD.

---

## Live API

The MVP is deployed on **Render**: [https://taskflow-api-np2z.onrender.com](https://taskflow-api-np2z.onrender.com)

- `GET /health` — liveness check
- Explore the endpoints with the [Postman collection](docs/taskflow.postman_collection.json) (set
  `baseUrl` to the URL above)

> _Demo GIF of the API flow (Swagger/Postman): coming soon._

---

## Domain Contract

The full entity model and business rules live in **[`docs/DOMAIN_RULES.md`](docs/DOMAIN_RULES.md)** —
the single source of truth kept in sync with the code across phases. The roadmap is in
**[`docs/MVP.md`](docs/MVP.md)**.

---

## Why This Stack

The choices here are deliberate, not defaults:

1. **Minimal APIs + Clean Architecture** — endpoints stay thin (bind + send to MediatR), and every
   business rule lives in `Application` or `Domain`, where it is unit-testable without HTTP or a
   database. The `Api` project has almost no logic.
2. **MediatR CQRS + pipeline behaviors** — commands and queries are separated, and **cross-cutting
   concerns are centralized as pipeline behaviors**: `ValidationBehavior` (FluentValidation) and
   `WorkspaceAuthorizationBehavior` (membership checks) run before every handler, so the isolation
   rule is written **once**, not repeated in each handler.
3. **Testcontainers + real Postgres** — integration tests run against a real database in Docker
   (or a provided service in CI), reaching **97.3% line coverage** of the codebase.

---

## Architecture

```
Api  →  Infrastructure  →  Application  →  Domain
```

`Domain` references nothing. `Application` references only `Domain`. `Infrastructure` implements the
interfaces defined in `Application`. `Api` wires everything via dependency injection.

```
src/
├── TaskFlow.Domain/          # Entities, enums, pure domain rules
├── TaskFlow.Application/     # Commands, queries, handlers (MediatR), validators, interfaces
├── TaskFlow.Infrastructure/  # EF Core, DbContext, repositories, token/password services
└── TaskFlow.Api/             # Minimal API endpoints, middlewares, DI
tests/
├── TaskFlow.UnitTests/       # Handlers, behaviors and validators in isolation (Moq)
└── TaskFlow.IntegrationTests/# Full HTTP flows against real Postgres (Testcontainers)
```

---

## Design Decisions Worth Highlighting

1. **One authorization pipeline, three scopes** — the `WorkspaceAuthorizationBehavior` resolves the
   workspace for any operation marked `IWorkspaceScoped`, `IProjectScoped` or `ITaskScoped` and
   checks membership before the handler runs. A user who is not a member gets `403` even if they
   know the resource ID.
2. **Last Admin protection** — a workspace can never be left without an Admin: removing or demoting
   the last Admin is rejected (409), at the application level, with the database as a safety net.
3. **The database is a safety net, never a silent destructor** — deleting a workspace cascades
   (explicit, Admin-only), while deleting a user is *blocked* by `Restrict`/`NO ACTION` FKs until
   the application resolves their workspaces, memberships and assignments.
4. **`TaskItem`, not `Task`** — avoids the collision with `System.Threading.Tasks.Task`.
5. **Fail fast** — JWT configuration is validated at startup (`ValidateOnStart`), migrations apply
   automatically on boot, and every error returns a standardized `{ type, message, errors, traceId }`.

---

## Getting Started (development)

### Requirements

- Docker
- [.NET 10 SDK](https://dotnet.microsoft.com/download)

### Setup

```bash
git clone https://github.com/vvasconceloss/taskflow-api.git
cd taskflow-api
```

Start PostgreSQL (Docker):

```bash
docker compose up -d
```

Restore and build:

```bash
dotnet restore
dotnet build
```

Configure local secrets (outside version control):

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" \
  "Host=localhost;Port=5433;Database=taskflow;Username=taskflow;Password=taskflow" \
  --project src/TaskFlow.Api

dotnet user-secrets set "Jwt:Secret" "dev-secret-0123456789abcdef0123456789abcdef" \
  --project src/TaskFlow.Api
```

Run the API:

```bash
dotnet run --project src/TaskFlow.Api
```

The API will be available at `http://localhost:5183` (check `/health` for a liveness check, and
`/swagger` for the OpenAPI UI in development).

---

## Testing

```bash
dotnet test
```

- **Unit tests** run in isolation with mocked dependencies (handlers, behaviors, validators).
- **Integration tests** run full HTTP flows against a real PostgreSQL — either a container spun up
  by Testcontainers, or the `POSTGRES_CONNECTION_STRING` environment variable when provided (how CI
  runs them against the workflow's Postgres service).

Current suite: **101 tests**, 100% passing, **97.3% line coverage** on the integration side.

---

## What I'd Do Differently / Next Steps

- **Refresh tokens** — short-lived access tokens + rotation (the MVP uses a single long-lived JWT).
- **Task comments and file attachments** — the natural next vertical slice.
- **Notifications** (email/push) for assignments and mentions.
- **Real-time updates** via SignalR when tasks change.
- **A frontend** — the API is fully REST; a web client (React/Blazor) would complete the product.

---

## License

This project is licensed under the MIT License.
