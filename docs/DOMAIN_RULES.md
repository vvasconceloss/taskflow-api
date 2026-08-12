# TaskFlow API — Domain Rules

> Reference document for the core business rules and entity model of TaskFlow API (MVP).
> Started during Phase 1 (Contracts and Business Rules) and kept in sync with the schema and APIs
> across phases. The domain entities (`src/TaskFlow.Domain/Entities`) are the source of truth for the
> data model; this document explains the *rules* on top of it.

---

## Entity Diagram

```
User (1) ──── (N) WorkspaceMember (N) ──── (1) Workspace (1) ──── (N) Project (1) ──── (N) TaskItem

TaskItem (N) ──── (0..1) User   # optional assignee; must be a member of the task's workspace
```

- A **User** is a member of many **Workspaces** through **WorkspaceMember** (N:N join), each
  membership carrying a **Role** (`Admin` or `Member`).
- A **Workspace** has many **Users** (as members) and many **Projects**.
- A **WorkspaceMember** is a join record linking one **User** to one **Workspace**; a user can have
  at most **one membership per workspace**.
- A **Project** belongs to exactly one **Workspace** and contains many **TaskItems**.
- A **TaskItem** belongs to exactly one **Project** and may optionally reference a **User** as the
  **assignee**. The assignee, when set, **must be a member of the project's workspace**.

---

## User

| Field         | Type     | Constraints                        |
|---------------|----------|-------------------------------------|
| id            | Guid     | Primary key                         |
| email         | string   | Unique, valid email format          |
| passwordHash  | string   | Never exposed; BCrypt hash          |
| name          | string?  | Optional display name               |
| createdAt     | DateTime | Auto-set at creation                |

### Rules

1. **Email uniqueness** — No two users can share the same email. Enforced at the database level
   (unique index).
2. **Password storage** — Password is never stored in plaintext. Must be hashed with BCrypt before
   persistence. Only the hash is stored.
3. **Password strength** — Between 8 and 72 characters (BCrypt limit) and must include at least one
   letter and one number (validated at the API layer via FluentValidation).
4. **Name is optional** — A user can register without providing a display name.
5. **Membership-scoped visibility** — A user never owns workspaces, projects or tasks directly. They
   only see resources through their `WorkspaceMember` records. All access is resolved via
   membership — see [Security Rules](#security-rules).
6. **Email format** — Must conform to a valid email structure (validated at the API layer).
7. **User deletion (DB semantics)** — The MVP exposes no user-deletion endpoint, but the database
   semantics are fixed: deleting a user is **blocked** while they have created workspaces
   (`Restrict`), hold any membership (`Restrict`), or are assigned to a task (`NO ACTION`).
   User deletion is an application-orchestrated operation (future): the app must resolve created
   workspaces, run the last-`Admin` check on every membership before removing it, and clear task
   assignments — the database acts as the safety net, never as a silent destructor.

---

## Workspace

| Field            | Type     | Constraints                          |
|------------------|----------|--------------------------------------|
| id               | Guid     | Primary key                          |
| name             | string   | 1–100 characters, required           |
| createdByUserId  | Guid     | FK to the creating User, required (restrict delete) |
| createdAt        | DateTime | Auto-set at creation                 |

### Rules

1. **Creator becomes Admin** — Whoever creates the workspace automatically becomes its first
   `Admin`: the workspace and the creator's `WorkspaceMember` (role `Admin`) are created in the
   same transaction.
2. **Admin-only management** — Only `Admin` members can: invite/remove members, change roles, and
   delete the workspace. Enforced in the handlers.
3. **Member permissions** — `Member` can create/edit projects and tasks, but cannot manage members.
4. **Name** — Required and must be between 1 and 100 characters.
5. **Cascade delete** — Deleting a workspace removes its memberships, projects and tasks
   (cascade). This is an explicit destructive action restricted to Admins.

---

## WorkspaceMember

| Field        | Type          | Constraints                          |
|--------------|---------------|--------------------------------------|
| id           | Guid          | Primary key                          |
| userId       | Guid          | FK to User, required                 |
| workspaceId  | Guid          | FK to Workspace, required            |
| role         | WorkspaceRole | `Admin` or `Member`                  |
| joinedAt     | DateTime      | Auto-set at creation                 |

### Rules

1. **Unique membership** — A user can have at most one membership per workspace. Enforced at the
   database level with a composite unique index on `(userId, workspaceId)`.
2. **Creator bootstrap** — The creator's `Admin` membership is created atomically with the
   workspace (see Workspace rule 1).
3. **Last Admin protection** — It is not allowed to remove — or demote to `Member` — the **last
   `Admin`** of a workspace. Without this rule the workspace would become orphaned. Enforced at the
   application level.
4. **Role transitions** — A role can only be changed by an `Admin` of the same workspace. Demoting
   or removing a user who is the last `Admin` is rejected.
5. **Cascade on workspace delete** — Memberships are removed when the workspace is deleted.
6. **Membership FK behavior** — The `userId` FK is `Restrict`: deleting a user is blocked while they
   hold memberships (see User rule 7). The `workspaceId` FK cascades on workspace deletion (rule 5).

---

## Project

| Field        | Type     | Constraints                              |
|--------------|----------|-------------------------------------------|
| id           | Guid     | Primary key                               |
| name         | string   | 1–100 characters, required                |
| description  | string?  | Optional, max 2000 characters             |
| workspaceId  | Guid     | FK to Workspace, required                 |
| isArchived   | bool     | Default `false`                           |
| createdAt    | DateTime | Auto-set at creation                      |

### Rules

1. **Workspace association** — A project belongs to exactly one workspace (`workspaceId` set at
   creation and immutable).
2. **Membership isolation** — Only members of the workspace can see, create, or edit projects in
   that workspace. Every query must go through the membership check (see Security Rules).
3. **Archive semantics** — Archiving sets `isArchived = true`. Archived projects do **not** appear
   in the default listing (`GET /workspaces/{workspaceId}/projects` returns non-archived projects
   by default), but remain accessible by ID.
4. **Blocked delete with tasks** — A project that still has tasks cannot be deleted. The user must
   archive or move/delete the tasks first. This favors explicit action over silent cascading
   deletion. Enforced at the application level.
5. **Name** — Required, 1–100 characters. **Description** — optional, max 2000 characters.

---

## TaskItem

| Field            | Type           | Constraints                                 |
|------------------|----------------|----------------------------------------------|
| id               | Guid           | Primary key                                  |
| title            | string         | 1–200 characters, required                   |
| description      | string?        | Optional, max 2000 characters                |
| status           | TaskStatus     | `Todo` (default), `InProgress`, `Done`       |
| priority         | TaskPriority   | `Low` (default), `Medium`, `High`            |
| projectId        | Guid           | FK to Project, required                      |
| assigneeUserId   | Guid?          | Optional FK to User; must be a workspace member |
| dueDate          | DateTime?      | Optional; date in the future when present    |
| createdAt        | DateTime       | Auto-set at creation                         |
| completedAt      | DateTime?      | Set when status becomes `Done`               |

### Rules

1. **Project association** — A task belongs to exactly one project.
2. **Membership isolation** — Only members of the project's workspace can see, create, or edit its
   tasks (see Security Rules).
3. **Assignee integrity** — `assigneeUserId`, when set, **must** be a member of the project's
   workspace. Validated at the application level — not just in the database. Deleting a user does
   **not** cascade to tasks: the assignee reference is cleared client-side (`ClientSetNull`); at
   the database the FK is `NO ACTION`, which blocks deleting a user still assigned to a task.
4. **CompletedAt state machine** — Changing `status` to `Done` automatically fills `completedAt`
   (server-side, never accepted from the client). Moving away from `Done` resets it to `null`.
5. **Status / priority** — Only the enum values are accepted (`Todo | InProgress | Done` and
   `Low | Medium | High`).
6. **Due date** — Optional; when provided, must be in the future (server time).

---

## Validation Summary

| Entity        | Field           | Rule                                      |
|---------------|-----------------|-------------------------------------------|
| User          | email           | Valid email format, unique                |
| User          | password        | 8–72 chars (BCrypt limit), ≥1 letter, ≥1 number |
| User          | name            | Optional; if provided, 1–100 chars        |
| Workspace     | name            | 1–100 chars, required                     |
| Project       | name            | 1–100 chars, required                     |
| Project       | description     | Optional; max 2000 chars                  |
| TaskItem      | title           | 1–200 chars, required                     |
| TaskItem      | description     | Optional; max 2000 chars                  |
| TaskItem      | status          | `Todo` / `InProgress` / `Done`            |
| TaskItem      | priority        | `Low` / `Medium` / `High`                 |
| TaskItem      | assigneeUserId  | Must be a member of the workspace         |
| TaskItem      | dueDate         | Optional; must be in the future           |
| WorkspaceMember | (userId, workspaceId) | Unique — one membership per user per workspace |

---

## Security Rules

1. **Membership isolation** — Every query that reads or writes a workspace, project, or task MUST
   verify that the authenticated user is a member of the target workspace. This is a
   **non-negotiable security rule**, not an implementation detail. The check should be centralized
   (an `IAuthorizationHandler` or a reusable MediatR pipeline behavior), not repeated manually in
   every handler.
2. **Admin checks** — Member-management operations (invite/remove/change role/delete workspace)
   additionally require the `Admin` role in that workspace.
3. **Password exposure** — `passwordHash` must never be returned in any API response.
4. **Authentication required** — All endpoints except `/health`, `/health/ready`, `/auth/register`
   and `/auth/login` require a valid JWT session.
5. **Input validation** — All user-supplied input is validated server-side (FluentValidation via
   the MediatR pipeline) before processing.

---

## Mental Test Cases

These scenarios are validated by the automated test suites and the manual QA (Phases 1, 9, 11):

1. **Last Admin removal** — Removing the last `Admin` of a workspace is rejected; a workspace never
   ends up with zero Admins.
2. **Last Admin demotion** — Demoting the last `Admin` to `Member` is rejected.
3. **Assignee outside workspace** — Assigning a task to a user who is not a member of the workspace
   fails.
4. **Duplicate membership** — Adding a user who is already a member of the workspace returns an
   error (no duplicate `WorkspaceMember`).
5. **Membership isolation** — User A creates a workspace/project/task. User B cannot see, edit, or
   delete it, even if User B knows the resource ID.
6. **Creator is Admin** — After creating a workspace, the creator holds exactly one membership with
   role `Admin`.
7. **Delete project with tasks** — Deleting a project that still has tasks is rejected; after the
   tasks are deleted, the same delete succeeds.
8. **Archived project** — An archived project disappears from the default listing but remains
   accessible by ID.
9. **CompletedAt rule** — Setting a task to `Done` fills `completedAt`; setting it back to `Todo`
   clears it; a `completedAt` sent by the client is ignored.
10. **Email uniqueness** — Registering with an existing email returns an error.
11. **Optional description** — Creating a project or task with only a name succeeds.
12. **Unauthenticated access** — Calling a protected endpoint without a JWT returns 401.
13. **User deletion blocked (DB semantics)** — Deleting a user who created workspaces, holds
    memberships, or is assigned to a task is blocked at the database level; the future app
    orchestration must resolve these before deleting the account.
