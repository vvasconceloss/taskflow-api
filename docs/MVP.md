# TaskFlow API — Complete MVP Plan

> Reference document for development, phase by phase, from Foundation to Deployment.
> Each phase includes: **objective**, **description/specs**, and **completion tasks** (checklist).

---

## Table of Contents

- [Product Overview](#product-overview)
- [Stack and Fixed Decisions](#stack-and-fixed-decisions)
- [Phase 0 — Foundation](#phase-0--foundation)
- [Phase 1 — Contracts and Business Rules](#phase-1--contracts-and-business-rules)
- [Phase 2 — Authentication](#phase-2--authentication)
- [Phase 3 — Workspaces and Members](#phase-3--workspaces-and-members)
- [Phase 4 — Projects](#phase-4--projects)
- [Phase 5 — Tasks](#phase-5--tasks)
- [Phase 6 — Advanced Listing (Filtering, Pagination, Sorting)](#phase-6--advanced-listing-filtering-pagination-sorting)
- [Phase 7 — Validation and Error Handling](#phase-7--validation-and-error-handling)
- [Phase 8 — Logging and Observability](#phase-8--logging-and-observability)
- [Phase 9 — Testing](#phase-9--testing)
- [Phase 10 — API Documentation](#phase-10--api-documentation)
- [Phase 11 — Security and Robustness](#phase-11--security-and-robustness)
- [Phase 12 — CI](#phase-12--ci)
- [Phase 13 — Docker and Deployment](#phase-13--docker-and-deployment)
- [Phase 14 — README and Presentation](#phase-14--readme-and-presentation)
- [MVP "Done" Definition](#mvp-done-definition)
- [Deliberately Out of Scope](#deliberately-out-of-scope)
- [Suggested Day-by-Day Distribution](#suggested-day-by-day-distribution)
- [Milestones Summary](#milestones-summary)

---

## Product Overview

TaskFlow is a task management API for teams, in the style of a simplified Linear/Trello "behind the scenes." A user creates a **Workspace**, invites members, organizes work into **Projects**, and within each project creates **Tasks** with status, priority, and an assignee.

The goal of the MVP is **not** to have a "wow" feature — it's to demonstrate, with depth, the patterns a .NET backend role expects: layered architecture, real authentication/authorization, domain modeling with clear rules, automated testing, containerization, and a CI pipeline. A UI is not part of the scope — the "proof" here is Swagger + the request collection + the tests.

```text
User
 └── WorkspaceMember (role: Admin | Member)
      └── Workspace
           └── Project
                └── TaskItem (assignee: optional User)
```

---

## Stack and Fixed Decisions

| Layer | Choice | Rationale |
|---|---|---|
| API | ASP.NET Core 10 — Minimal APIs | Most modern standard, less boilerplate than Controllers, same capability |
| Architecture | Clean Architecture (Domain / Application / Infrastructure / API) | Separation of concerns, testable, exactly what the role asks for |
| CQRS | MediatR | Separates commands (writes) from queries (reads), reduces coupling between endpoints and business logic |
| ORM | Entity Framework Core + PostgreSQL | Market standard; Postgres is the most cited database in 2026 job postings |
| Validation | FluentValidation | Declarative validation, integrates well with MediatR via a pipeline behavior |
| Auth | JWT Bearer (no cookie — pure API, testable directly in Swagger/Postman) | Simple, correct decision for a standalone API with no dedicated frontend |
| Testing | xUnit + FluentAssertions + Testcontainers | Fast unit tests + integration tests against a real Postgres running in a container |
| Logging | Serilog (console + JSON file) | Structured logging, market standard |
| Containerization | Docker + docker-compose | API + Postgres spin up together with a single command |
| CI | GitHub Actions | Build + tests + lint on every PR |
| Deployment | Render or Railway (free tier) | Real, free deployment with a managed Postgres |

> **Naming note:** the task entity will be called `TaskItem` in code (not `Task`), because `Task` collides with `System.Threading.Tasks.Task`. This is intentional and worth mentioning in the README — it's the kind of detail that shows attention to real pitfalls of the language.

---

## Phase 0 — Foundation

### Objective
Have an executable base: .NET solution with Clean Architecture layers, Postgres running via Docker, and a health-check endpoint responding.

### Specs — Solution structure

```text
TaskFlow.sln
├── src/
│   ├── TaskFlow.Domain/          # Entities, enums, pure domain rules (no external dependencies)
│   ├── TaskFlow.Application/     # Commands, Queries, Handlers (MediatR), Validators, DTOs, Interfaces
│   ├── TaskFlow.Infrastructure/  # EF Core, DbContext, Repositories, concrete implementations
│   └── TaskFlow.Api/             # Minimal API endpoints, Program.cs, middlewares, DI
└── tests/
    ├── TaskFlow.UnitTests/
    └── TaskFlow.IntegrationTests/
```

Dependency rule (the most important one in Clean Architecture):

```text
Api  →  Infrastructure  →  Application  →  Domain
```

`Domain` references nothing. `Application` only references `Domain`. `Infrastructure` implements interfaces defined in `Application`. `Api` knows about everyone, but only via dependency injection.

### Specs — Docker Compose (skeleton)

```yaml
services:
  postgres:
    image: postgres:16
    environment:
      POSTGRES_DB: taskflow
      POSTGRES_USER: taskflow
      POSTGRES_PASSWORD: taskflow
    ports:
      - "5433:5432"
    volumes:
      - pgdata:/var/lib/postgresql/data

volumes:
  pgdata:
```

### Tasks
- [x] Create the `TaskFlow.sln` solution with the 4 layer projects + 2 test projects
- [x] Configure references between projects respecting the dependency rule
- [x] Add base NuGet packages (MediatR, FluentValidation, EF Core, Npgsql, Serilog)
- [x] Create `docker-compose.yml` with the Postgres service
- [x] Confirm Postgres starts correctly (`docker compose up -d`)
- [x] Create an empty `DbContext` in `Infrastructure`
- [x] Configure connection string via `appsettings.Development.json` + environment variable
- [x] Implement `GET /health` endpoint returning status 200
- [x] Confirm the API starts locally (`dotnet run`) and responds at `/health`
- [x] Create `.gitignore` (.NET template + `.env`)
- [x] Create initial `README.md` (placeholder, expanded in Phase 14)
- [x] First commit

### Completion criteria
The solution builds, Postgres starts via Docker, and `GET /health` responds 200 locally.

---

## Phase 1 — Contracts and Business Rules

### Objective
Before writing any endpoint, define the data model and the business rules that will govern the entire MVP.

### Specs — Domain model

```text
User
 └── WorkspaceMember (N:N between User and Workspace, with role)
      └── Workspace
           └── Project
                └── TaskItem
```

```csharp
public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string Name { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<WorkspaceMember> Memberships { get; set; } = [];
}

public enum WorkspaceRole { Admin, Member }

public class Workspace
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<WorkspaceMember> Members { get; set; } = [];
    public ICollection<Project> Projects { get; set; } = [];
}

public class WorkspaceMember
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid WorkspaceId { get; set; }
    public WorkspaceRole Role { get; set; }
    public DateTime JoinedAt { get; set; }
}

public class Project
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public Guid WorkspaceId { get; set; }
    public bool IsArchived { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<TaskItem> Tasks { get; set; } = [];
}

public enum TaskStatus { Todo, InProgress, Done }
public enum TaskPriority { Low, Medium, High }

public class TaskItem
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }
    public TaskStatus Status { get; set; }
    public TaskPriority Priority { get; set; }
    public Guid ProjectId { get; set; }
    public Guid? AssigneeUserId { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
```

### Specs — Business rules

**User**
- `email` is unique.
- Password is never stored in plaintext (hashed with BCrypt).
- A user can only see workspaces they are a member of.

**Workspace**
- Whoever creates the workspace automatically becomes `Admin` (first `WorkspaceMember`).
- Only `Admin` can: invite/remove members, change roles, delete the workspace.
- `Member` can: create/edit projects and tasks, but not manage members.

**WorkspaceMember**
- A user can only have one membership per workspace (`@@unique(UserId, WorkspaceId)` equivalent in EF Core: composite unique index).
- It is not allowed to remove the last `Admin` from a workspace (critical rule — without it, the workspace would become orphaned).

**Project**
- Belongs to exactly one workspace.
- Only members of the workspace can see/create/edit projects in that workspace.
- Archived projects do not appear in the default listing, but remain accessible by ID.

**TaskItem**
- Belongs to exactly one project.
- `AssigneeUserId`, if set, **must** be a member of the project's workspace (integrity rule validated at the application level, not just in the database).
- Setting `Status` to `Done` automatically fills `CompletedAt`; moving away from `Done` resets it to `null`.

### Specs — Critical isolation rule (same logic as LifeOS, adapted)

Wrong:
```csharp
var task = await _dbContext.Tasks.FindAsync(taskId);
```

Correct:
```csharp
var task = await _dbContext.Tasks
    .Include(t => t.Project)
    .Where(t => t.Id == taskId)
    .Where(t => t.Project.Workspace.Members.Any(m => m.UserId == currentUserId))
    .FirstOrDefaultAsync();
```

Every access to `Project` or `TaskItem` **must** go through a check that the authenticated user is a member of the corresponding workspace. This check should become an `IAuthorizationHandler` or a reusable MediatR pipeline behavior — not be manually repeated in every handler.

### Tasks
- [x] Create the domain entities in `TaskFlow.Domain` (User, Workspace, WorkspaceMember, Project, TaskItem)
- [x] Document the business rules in `/docs/DOMAIN_RULES.md`
- [x] Mentally validate edge cases (e.g., "can I remove the last admin?", "can I assign a task to someone outside the workspace?")
- [x] Design the `EntityTypeConfiguration` (EF Core Fluent API) for each entity, including unique indexes
- [x] Review and approve the model before moving to Phase 2

### Completion criteria
An approved domain document exists, with the entity model and business rules written down, before any endpoint is implemented.

---

## Phase 2 — Authentication

### Objective
A user can create an account and authenticate via the API. First real vertical slice of the product.

### Specs — Endpoints

| Method | Route | Description |
|---|---|---|
| POST | `/auth/register` | Creates a new user |
| POST | `/auth/login` | Authenticates and returns a JWT token |
| GET | `/auth/me` | Returns the authenticated user |

Example `POST /auth/register`:
```json
// Request
{ "name": "Victor", "email": "victor@example.com", "password": "strong-password" }

// Response (201)
{ "id": "guid", "name": "Victor", "email": "victor@example.com" }
```

Example `POST /auth/login`:
```json
// Request
{ "email": "victor@example.com", "password": "strong-password" }

// Response (200)
{ "token": "eyJhbGciOi...", "expiresAt": "2026-08-15T12:00:00Z" }
```

### Specs — Authentication decision
- Mechanism: **JWT Bearer** in the `Authorization: Bearer {token}` header.
- No refresh token in the MVP (documented as a next step — don't let this block development).
- Password hashing: BCrypt.Net-Next.
- Token claims: `sub` (userId), `email`, `name`.

### Specs — Module structure (Application)

```text
TaskFlow.Application/Features/Auth/
├── Register/
│   ├── RegisterCommand.cs
│   ├── RegisterCommandHandler.cs
│   └── RegisterCommandValidator.cs
├── Login/
│   ├── LoginCommand.cs
│   └── LoginCommandHandler.cs
└── GetMe/
    ├── GetMeQuery.cs
    └── GetMeQueryHandler.cs
```

### Tasks
- [x] Configure Identity/JWT (`Microsoft.AspNetCore.Authentication.JwtBearer`)
- [x] Implement `RegisterCommand` + handler (password hashing, duplicate email check)
- [x] Implement `LoginCommand` + handler (password verification, JWT generation)
- [x] Implement `GetMeQuery` + handler
- [x] Map Minimal API endpoints: `POST /auth/register`, `POST /auth/login`, `GET /auth/me`
- [x] Configure authentication/authorization middleware in `Program.cs`
- [ ] Write tests:
  - [x] Register a user successfully
  - [x] Reject a duplicate email
  - [ ] Reject a weak password (minimum validation rule)
  - [x] Log in successfully
  - [x] Reject invalid credentials
  - [x] Access `/auth/me` while authenticated
  - [x] Reject `/auth/me` without a token

### Completion criteria
All authentication tests pass, and it is possible to register, authenticate, and validate a session via direct calls (Swagger/Postman/curl).

---

## Phase 3 — Workspaces and Members

### Objective
The authenticated user can create workspaces and manage who is part of them.

### Specs — Endpoints

| Method | Route | Description |
|---|---|---|
| GET | `/workspaces` | Lists the authenticated user's workspaces |
| POST | `/workspaces` | Creates a workspace (creator becomes Admin) |
| GET | `/workspaces/{id}` | Workspace detail |
| PATCH | `/workspaces/{id}` | Edits the name (Admin only) |
| DELETE | `/workspaces/{id}` | Removes the workspace (Admin only) |
| POST | `/workspaces/{id}/members` | Invites/adds a member by email (Admin only) |
| PATCH | `/workspaces/{id}/members/{userId}` | Changes a member's role (Admin only) |
| DELETE | `/workspaces/{id}/members/{userId}` | Removes a member (Admin only; blocked if it's the last Admin) |

### Specs — Authorization
Create a MediatR `IPipelineBehavior<TRequest, TResponse>` (`WorkspaceAuthorizationBehavior`) that, for commands/queries marked with an `IWorkspaceScoped { Guid WorkspaceId }` interface, automatically checks whether the authenticated user is a member (and, when needed, an Admin) of that workspace — **before** the handler runs. This avoids repeating the check manually in every handler (same principle as the isolation rule from Phase 1).

### Tasks
- [x] Create the `EntityTypeConfiguration` for `Workspace` and `WorkspaceMember` + migration
- [x] Implement `CreateWorkspaceCommand` (creates the workspace + the Admin `WorkspaceMember` in the same transaction)
- [x] Implement `ListMyWorkspacesQuery`
- [x] Implement `GetWorkspaceQuery`
- [x] Implement `UpdateWorkspaceCommand` (Admin only)
- [x] Implement `DeleteWorkspaceCommand` (Admin only)
- [x] Implement `AddMemberCommand` (by email; error if user doesn't exist or is already a member)
- [x] Implement `UpdateMemberRoleCommand`
- [x] Implement `RemoveMemberCommand` with the "don't remove the last Admin" rule
- [x] Create `IWorkspaceScoped` + `WorkspaceAuthorizationBehavior` (MediatR pipeline)
- [x] Tests: creation, listing isolated per user, editing restricted to Admin, removal of the last Admin blocked
- [x] Isolation tests: user A cannot see/edit user B's workspace

### Completion criteria
A user can create workspaces, invite members, change roles, and the "don't remove the last Admin" rule is covered by an automated test.

---

## Phase 4 — Projects

### Objective
Within a workspace, members can organize work into projects.

### Specs — Endpoints

| Method | Route | Description |
|---|---|---|
| GET | `/workspaces/{workspaceId}/projects` | Lists the workspace's projects |
| POST | `/workspaces/{workspaceId}/projects` | Creates a project |
| GET | `/projects/{id}` | Project detail |
| PATCH | `/projects/{id}` | Edits a project |
| POST | `/projects/{id}/archive` | Archives a project |
| DELETE | `/projects/{id}` | Removes a project (blocked if it has tasks — same protection logic used for Pillars in LifeOS) |

### Specs — Business rule
It is not allowed to delete a project that still has associated tasks (the user must archive or move the tasks first). This decision favors explicit action over silent cascading deletion.

### Tasks
- [x] Create the `EntityTypeConfiguration` for `Project` + migration
- [x] Implement `CreateProjectCommand`
- [x] Implement `ListProjectsQuery` (by workspace, hiding archived by default)
- [x] Implement `GetProjectQuery`
- [x] Implement `UpdateProjectCommand`
- [x] Implement `ArchiveProjectCommand`
- [x] Implement `DeleteProjectCommand` with the "no associated tasks" validation
- [x] Tests for all endpoints, including the deletion error case
- [x] Isolation tests: a member of another workspace cannot access the project

### Completion criteria
Full project CRUD working, with the deletion rule enforced and covered by tests.

---

## Phase 5 — Tasks

### Objective
The core feature of the product: creating and managing tasks within a project.

### Specs — Endpoints

| Method | Route | Description |
|---|---|---|
| GET | `/projects/{projectId}/tasks` | Lists the project's tasks |
| POST | `/projects/{projectId}/tasks` | Creates a task |
| GET | `/tasks/{id}` | Task detail |
| PATCH | `/tasks/{id}` | Edits a task (title, description, priority, due date) |
| PATCH | `/tasks/{id}/status` | Changes status (Todo/InProgress/Done) |
| PATCH | `/tasks/{id}/assignee` | Assigns/removes an assignee |
| DELETE | `/tasks/{id}` | Removes a task |

Creation example:
```json
{
  "title": "Set up CI pipeline",
  "description": "GitHub Actions with build + test",
  "priority": "High",
  "dueDate": "2026-08-15"
}
```

### Specs — Business rule
- Changing `Status` to `Done` automatically fills `CompletedAt` (server-side, never accepted from the client).
- `AssigneeUserId` can only be a `UserId` that is a member of the same workspace as the project — validated in the handler before persisting.

### Tasks
- [x] Create the `EntityTypeConfiguration` for `TaskItem` + migration
- [x] Implement `CreateTaskCommand`
- [x] Implement `ListTasksQuery` (by project)
- [x] Implement `GetTaskQuery`
- [x] Implement `UpdateTaskCommand`
- [x] Implement `UpdateTaskStatusCommand` (with the `CompletedAt` rule)
- [x] Implement `UpdateTaskAssigneeCommand` (with the membership validation)
- [x] Implement `DeleteTaskCommand`
- [x] Tests for all endpoints, including the two business rules above
- [x] Test: assigning a task to a non-member should fail

### Completion criteria
Full task CRUD, with the two main business rules covered by automated tests.

---

## Phase 6 — Advanced Listing (Filtering, Pagination, Sorting)

### Objective
Make the listing endpoints ready for real use, not just a `SELECT *`.

### Specs — Standard query string

```text
GET /projects/{projectId}/tasks?status=InProgress&priority=High&assigneeId={guid}&page=1&pageSize=20&sortBy=dueDate&sortDir=asc
```

### Specs — Paginated response format

```json
{
  "items": [ /* ... */ ],
  "page": 1,
  "pageSize": 20,
  "totalItems": 57,
  "totalPages": 3
}
```

### Tasks
- [ ] Create a generic `PagedResult<T>` in `Application`
- [ ] Implement filters by `status`, `priority`, `assigneeId` in `ListTasksQuery`
- [ ] Implement pagination (`page`, `pageSize`, with a maximum `pageSize` limit to prevent abuse)
- [ ] Implement sorting (`sortBy`, `sortDir`) with a whitelist of allowed fields (never accept a free-form column name from the client)
- [ ] Apply the same pattern to `ListProjectsQuery` and `ListMyWorkspacesQuery`
- [ ] Tests: combined filters, pagination on an empty page, sorting in both directions, whitelist rejecting an invalid field

### Completion criteria
All listing endpoints consistently support filtering, pagination, and sorting, and this is covered by tests.

---

## Phase 7 — Validation and Error Handling

### Objective
Consistent error responses and input validation on 100% of commands.

### Specs — Validation pipeline (MediatR + FluentValidation)

```csharp
public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var failures = validators
            .Select(v => v.Validate(request))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count != 0)
            throw new ValidationException(failures);

        return await next();
    }
}
```

### Specs — Standard error format

```json
{
  "type": "ValidationError",
  "message": "One or more validation errors occurred.",
  "errors": {
    "title": ["Title is required."],
    "dueDate": ["Due date must be in the future."]
  },
  "traceId": "00-abc123..."
}
```

### Specs — Global exception middleware
Map known exceptions to the correct status codes:

| Exception | Status |
|---|---|
| `ValidationException` | 400 |
| `NotFoundException` | 404 |
| `ForbiddenException` (not a member/not an Admin) | 403 |
| `ConflictException` (e.g., duplicate email, removing last Admin) | 409 |
| Anything else | 500 (full log, generic response to the client) |

### Tasks
- [ ] Create `ValidationBehavior` and register it in the MediatR pipeline
- [ ] Write a `Validator` (FluentValidation) for **every** existing Command so far
- [ ] Create domain exceptions: `NotFoundException`, `ForbiddenException`, `ConflictException`
- [ ] Implement global exception handling middleware (`IExceptionHandler` from ASP.NET Core)
- [ ] Standardize the error response format across the entire API
- [ ] Tests: each validator rejects invalid input as expected
- [ ] Test: an unhandled exception returns 500 without leaking a stack trace to the client

### Completion criteria
Every command has validation, and every known exception is mapped to a consistent HTTP status and error format.

---

## Phase 8 — Logging and Observability

### Objective
Have visibility into what's happening in the API, both in development and (eventually) in production.

### Specs
- Serilog configured with a console sink (development) and a JSON file sink (production).
- Structured logs including: `userId` (when authenticated), `traceId`, route, status code, response time.
- Request/response logging middleware (never logging a password body).

### Tasks
- [ ] Configure Serilog in `Program.cs` (console + file)
- [ ] Add request logging middleware (`UseSerilogRequestLogging`)
- [ ] Enrich logs with `userId` when authenticated
- [ ] Confirm no log exposes a password, password hash, or full JWT token
- [ ] Manually validate the logs in a full flow (register → login → create task)

### Completion criteria
Structured logs appear in the console on every call, with no sensitive data exposed.

---

## Phase 9 — Testing

### Objective
Test coverage that proves the critical business rules actually work — not "coverage for coverage's sake."

### Specs — Testing strategy

```text
TaskFlow.UnitTests/
  → Tests handlers in isolation, with mocked repositories (Moq or NSubstitute)
  → Focus: pure business logic (CompletedAt calculation, last-Admin rule, etc.)

TaskFlow.IntegrationTests/
  → Uses Testcontainers to spin up a real Postgres in Docker during the test run
  → Focus: full flow over HTTP (WebApplicationFactory) — register → login → create workspace → create project → create task
```

### Specs — Integration test example (structure)

```csharp
public class TaskFlowTests : IClassFixture<TaskFlowApiFactory>
{
    [Fact]
    public async Task Should_Create_Task_And_Return_201()
    {
        var client = await _factory.CreateAuthenticatedClientAsync();
        var workspace = await client.PostAsJsonAsync("/workspaces", new { name = "Acme" });
        // ... creates project, creates task, validates 201 response and content
    }
}
```

### Tasks
- [ ] Configure Testcontainers in the integration test project
- [ ] Create a custom `WebApplicationFactory` (`TaskFlowApiFactory`) pointing to the container's Postgres
- [ ] Write unit tests for the critical business rules:
  - [ ] Don't remove the last Admin of a workspace
  - [ ] `CompletedAt` is correctly filled/cleared when status changes
  - [ ] Assigning a task to a non-member fails
  - [ ] Deleting a project with tasks fails
- [ ] Write at least 3 end-to-end integration tests covering complete flows
- [ ] Write user/workspace isolation tests (already partly covered in earlier phases — consolidate here)
- [ ] Run the full test suite locally and confirm 100% pass
- [ ] (Optional, if time allows) Configure a coverage report (Coverlet)

### Completion criteria
The test suite runs locally with success and covers the domain's critical business rules, not just the "happy paths."

---

## Phase 10 — API Documentation

### Objective
Anyone (including a recruiter) can open Swagger and understand/test the API without reading the code.

### Specs
- Swagger/OpenAPI with response annotations for every endpoint (200, 400, 401, 403, 404, 409).
- Request/response examples in Swagger (via `Swashbuckle` annotations or `.WithOpenApi()` on Minimal APIs).
- Postman/Bruno collection exported and versioned in `/docs/taskflow.postman_collection.json`.
- Swagger's "Authorize" button configured for JWT Bearer.

### Tasks
- [ ] Configure Swashbuckle/Swagger with JWT Bearer support in the "Authorize" button
- [ ] Add a summary and description for each Minimal API endpoint
- [ ] Document the main error codes per endpoint
- [ ] Export and version the Postman/Bruno collection with real examples
- [ ] Test the full flow using only Swagger, from scratch (register → login → workspace → project → task)

### Completion criteria
It is possible to test the entire API flow using only Swagger, without needing to read the source code.

---

## Phase 11 — Security and Robustness

### Objective
Prepare the API to handle real data and real users before deployment.

### Specs
- Rate limiting on `/auth/login` (basic brute-force protection).
- CORS correctly configured for production (no wildcard).
- Never expose `PasswordHash` in any response DTO.
- Audit **all** queries to guarantee filtering by the authenticated `userId`/`workspaceId`.
- Secrets (connection string, JWT secret) never committed — via environment variables / `dotnet user-secrets` in dev.

### Tasks
- [ ] Configure rate limiting on `/auth/login` (`Microsoft.AspNetCore.RateLimiting`)
- [ ] Configure production CORS (specific origin, no wildcard)
- [ ] Audit all response DTOs to ensure `PasswordHash` is never serialized
- [ ] Audit **all** EF Core queries confirming filtering by membership (Workspaces, Projects, Tasks)
- [ ] Confirm secrets are outside version control (`.gitignore`, `user-secrets`, environment variables)
- [ ] Write specific user isolation tests (if not already covered)
- [ ] Review error messages to avoid leaking internal detail (stack trace, table name, etc.)

### Completion criteria
Security audit completed, with automated tests confirming isolation between users/workspaces and no sensitive data exposure.

---

## Phase 12 — CI

### Objective
Ensure all code that reaches `main` builds, passes tests, and is lint-clean.

### Specs — Pipeline

```text
Pull Request
     ├── dotnet restore
     ├── dotnet build --no-restore
     ├── dotnet test --no-build
     └── dotnet format --verify-no-changes
```

### Specs — Workflow (GitHub Actions)

```yaml
name: CI

on:
  pull_request:
  push:
    branches: [main]

jobs:
  build-and-test:
    runs-on: ubuntu-latest
    services:
      postgres:
        image: postgres:16
        env:
          POSTGRES_PASSWORD: postgres
        ports: ["5432:5432"]
        options: >-
          --health-cmd pg_isready
          --health-interval 10s
          --health-timeout 5s
          --health-retries 5
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: "10.0.x"
      - run: dotnet restore
      - run: dotnet build --no-restore --configuration Release
      - run: dotnet test --no-build --configuration Release
      - run: dotnet format --verify-no-changes
```

### Tasks
- [ ] Create `.github/workflows/ci.yml`
- [ ] Configure trigger on pull requests and push to `main`
- [ ] Configure a Postgres service in the workflow for integration tests
- [ ] Confirm `dotnet test` runs unit **and** integration tests in CI
- [ ] Confirm the pipeline correctly fails when a test breaks (test this on purpose)
- [ ] Configure branch protection on `main` requiring a green CI before merge
- [ ] Validate the pipeline with a test PR

### Completion criteria
No code reaches `main` without passing through automated build, tests, and format check.

---

## Phase 13 — Docker and Deployment

### Objective
Put the API into production, publicly accessible.

### Specs — Multi-stage Dockerfile

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish src/TaskFlow.Api -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENTRYPOINT ["dotnet", "TaskFlow.Api.dll"]
```

### Specs — Pre-deploy checklist
- Environment variables configured (connection string, JWT secret, CORS origin).
- Migrations applied in production (via `dotnet ef database update` or automatic migration on startup, documented).
- HTTPS guaranteed by the hosting platform.
- Minimal logging configured in the production environment.

### Tasks
- [ ] Create a multi-stage `Dockerfile`
- [ ] Validate the image locally (`docker build` + `docker run`)
- [ ] Choose a deployment platform (Render or Railway, free tier)
- [ ] Choose a managed Postgres (Neon, Supabase, or the platform's own Postgres)
- [ ] Configure production environment variables
- [ ] Run migrations in production
- [ ] Configure production CORS with the correct origin (if there is an external client/Swagger)
- [ ] Deploy
- [ ] Test the full flow in production via Swagger/Postman: register → login → workspace → project → task
- [ ] Document the public URL in the README

### Completion criteria
The API is publicly accessible via HTTPS, with the full flow validated in production.

---

## Phase 14 — README and Presentation

### Objective
Turn the code into a portfolio project that explains itself.

### Specs — README structure
1. Title + one sentence describing the problem solved
2. GIF/screenshot of Swagger in action
3. Link to the production API + link to the Postman/Bruno collection
4. Stack and **why** each choice was made (2-3 justified decisions, not just a list)
5. Simple architecture diagram (Domain/Application/Infrastructure/API)
6. How to run locally (`docker compose up` + `dotnet run`)
7. How to run the tests
8. Design decisions worth highlighting (e.g., the per-workspace authorization pipeline, the last-Admin rule, `TaskItem` instead of `Task`)
9. "What I'd do differently/next steps" (refresh token, notifications, task comments, etc.)

### Tasks
- [ ] Write the complete README following the structure above
- [ ] Record a short GIF of the Swagger flow
- [ ] Review all commits (clear messages, not too many loose "fix", "wip")
- [ ] Add a license (MIT, for example)
- [ ] Pin the repository on your GitHub profile
- [ ] Write a short LinkedIn post about a specific technical decision from the project (not "finished a CRUD")

### Completion criteria
Someone who has never seen the project can, just by reading the README, understand what it does, why it was built that way, and run it locally within a few minutes.

---

## MVP "Done" Definition

The MVP is done when it is possible to:

1. Register a user.
2. Log in and obtain a JWT token.
3. Create a workspace (and automatically become Admin).
4. Invite a second user as a member.
5. Create a project within the workspace.
6. Create tasks within the project.
7. Assign a task to a member.
8. Move a task's status all the way to `Done`.
9. List tasks with filtering, pagination, and sorting.
10. See the entire flow documented and testable via Swagger.
11. Run the test suite successfully.
12. Access the API publicly in production.

### Final MVP composition

```text
Authentication (JWT)
      +
Workspaces and Members (with roles)
      +
Projects
      +
Tasks (with status, priority, assignee)
      +
Advanced listing (filtering/pagination/sorting)
      +
Consistent validation and error handling
      +
Testing (unit + integration)
      +
CI
      +
Deployment
```

---

## Deliberately Out of Scope

```text
Frontend/UI
Task comments
File attachments
Notifications (email/push)
Real-time collaboration (SignalR/WebSockets)
Refresh token / session rotation
Multiple workspaces with advanced billing/multi-tenancy
Kanban board with drag-and-drop
AI integration (reserved for the second project — DevPulse)
Mobile app
```

These items should only be considered once the core project is published and serving as a portfolio piece — not before.

---

## Suggested Day-by-Day Distribution

| Day | Phases covered |
|---|---|
| 1 | Phase 0 (Foundation) + Phase 1 (Domain) |
| 2 | Phase 2 (Authentication) + Phase 3 (Workspaces and Members) |
| 3 | Phase 4 (Projects) + Phase 5 (Tasks) |
| 4 | Phase 6 (Advanced Listing) + Phase 7 (Validation/Errors) + Phase 8 (Logging) |
| 5 | Phase 9 (Testing) |
| 6 | Phase 10 (Documentation) + Phase 11 (Security) + Phase 12 (CI) + Phase 13 (Deployment) |
| 7 (buffer) | Phase 14 (README/Presentation) + slack for the unexpected |

---

## Milestones Summary

```text
M0  — Foundation                        [ ]
M1  — Domain and Rules                  [ ]
M2  — Authentication                    [ ]
M3  — Workspaces and Members            [ ]
M4  — Projects                          [ ]
M5  — Tasks                             [ ]
M6  — Advanced Listing                  [ ]
M7  — Validation and Errors             [ ]
M8  — Logging                           [ ]
M9  — Testing                           [ ]
M10 — API Documentation                 [ ]
M11 — Security and Robustness           [ ]
M12 — CI                                [ ]
M13 — Deployment                        [ ]
M14 — README and Presentation           [ ]
M15 — MVP Release                       [ ]
```
