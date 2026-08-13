<div align="center">

# TaskFlow API

> A task management API for teams — workspaces, projects and tasks, built with Clean Architecture on .NET.

<p align="center">

[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-10-512bd4.svg)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-13-239120.svg)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-336791.svg)](https://www.postgresql.org/)
[![Status](https://img.shields.io/badge/status-in%20development-orange.svg)](docs/MVP.md)

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

## Planned Features

- [x] **Authentication** — JWT-based register/login
- [x] **Workspaces & Members** — role-based access (Admin/Member)
- [x] **Projects** — organized per workspace, with archiving
- [x] **Tasks** — status, priority, assignee, due date
- [ ] **Advanced listing** — filtering, pagination, sorting on all collections
- [ ] **Consistent validation & error handling**
- [ ] **Automated tests** — unit + integration (Testcontainers)
- [ ] **CI pipeline** — build, test and lint on every PR
- [ ] **Dockerized & deployed** — publicly accessible API

---

## Tech Stack

- ASP.NET Core (Minimal APIs)
- Entity Framework Core + PostgreSQL
- MediatR (CQRS)
- FluentValidation
- JWT Bearer authentication
- xUnit + FluentAssertions + Testcontainers
- Serilog (structured logging)
- Docker + docker-compose
- GitHub Actions (CI)

---

## Project Structure

```
taskflow/
│
├── docs/
│   └── MVP.md                    # full MVP roadmap and domain rules
│
├── src/
│   ├── TaskFlow.Domain/          # Entities, enums, pure domain rules
│   ├── TaskFlow.Application/     # Commands, queries, handlers (MediatR), validators, DTOs
│   ├── TaskFlow.Infrastructure/  # EF Core, DbContext, repositories
│   └── TaskFlow.Api/             # Minimal API endpoints, Program.cs, middlewares
│
├── tests/
│   ├── TaskFlow.UnitTests/
│   └── TaskFlow.IntegrationTests/
│
├── .gitginore
├── docker-compose.yml
├── LICENSE
├── README.md
└── TaskFlow.sln
```

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

Run the API:

```bash
dotnet run --project src/TaskFlow.Api
```

The API will be available at `http://localhost:5183` (check `/health` for a liveness check).

---

## Testing

```bash
dotnet test
```

Unit tests run in isolation with mocked dependencies. Integration tests spin up a real PostgreSQL
instance via Testcontainers, so Docker must be running.

---

## License

This project is licensed under the MIT License.
