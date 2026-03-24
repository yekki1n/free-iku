You are a senior .NET 8 developer working on a greenfield project. 
I am the tech lead. You will receive requirements, constraints, and acceptance criteria.
You decide HOW to implement — I define WHAT and WHY.

When you start a new file or module, first state your implementation plan in 2-3 lines, then code it.
Keep every file consistent with the patterns you establish in the first files you write.
If you are unsure about a design decision, pick the simpler option and note it.

---

## PROJECT OVERVIEW

Build a RESTful Task Management API. The system manages Users, Projects, and Tasks.
Users can belong to multiple Projects. Tasks belong to a Project and can be assigned to a User.
Business rules around task assignment and status transitions must be enforced at the service layer.

---

## TECH STACK & ENVIRONMENT

- .NET 8, ASP.NET Core Web API
- Entity Framework Core 8, SQL Server
- SQL Server runs in Docker: server name = localhost,1433 | DB = TaskManagementDB | user = sa | password = @AAAaaa123123 | TrustServerCertificate=True
- Authentication: JWT Bearer
- Password hashing: BCrypt.Net-Next
- API documentation: Swashbuckle (Swagger), with JWT support in the UI
- Unit testing: xUnit + Moq
- macOS host

All NuGet packages must be compatible with .NET 8. Pick stable, widely-used versions.

---

## SOLUTION STRUCTURE

Two projects in one solution:
1. TaskManagementAPI — the main API
2. TaskManagementAPI.Tests — unit tests

For the API project, establish a clean layered folder structure.
Naming and folder conventions you define in the first file must be followed throughout the entire project.
Do not mix responsibilities between layers.

---

## DOMAIN

Three main entities: User, Project, Task.
There is also a join entity for the many-to-many between User and Project.

Decide field names, data types, and nullability yourself — they must correctly model these business rules:
- A User has a role: either USER or MANAGER.
- A Project is owned by one User (must be MANAGER).
- A Task belongs to one Project. A Task can optionally be assigned to one User.
- A User can only be assigned a Task if that User is a member of the Task's Project.
- A Task has a status: TODO → IN_PROGRESS → DONE. Once DONE, status cannot be changed.
- Task deadline, if provided, must be a future date.

Model these rules in your entity and enum design. Store enums as strings in the database.
Avoid naming conflicts with System.Threading.Tasks.Task and System.Threading.Tasks.TaskStatus — rename your entities or enums to prevent compile errors.

---

## DATA LAYER

Use EF Core Code-First. One DbContext, explicit FK configuration, unique index on User email.
Auto-apply pending migrations on startup.

Seed the database on first run (only if empty):
- At least 5 Users (mix of MANAGER and USER roles), passwords hashed with BCrypt
- At least 4 Projects
- Users distributed across projects via the join table
- At least 35 Tasks with varied statuses, some assigned, some not

---

## ARCHITECTURE CONSTRAINTS

- Repository pattern: one interface + one implementation per entity. No DbContext in controllers or services.
- Service layer: all business logic here. Services depend on repository interfaces, not implementations.
- Controllers: thin. Only handle HTTP concerns (routing, status codes, request/response mapping). No logic.
- Register all dependencies as Scoped.
- Use constructor injection throughout.

---

## API DESIGN

Standard response envelope for all endpoints:
  { "code": <httpStatusCode>, "message": "<string>", "data": <payload or null> }

Use DTOs for all request and response bodies — never expose entity classes directly.
Validate all inputs using DataAnnotations on DTOs. Validate at the controller boundary.

Endpoints to implement:

  Auth (public):
    POST /api/auth/register
    POST /api/auth/login  → returns JWT token

  Users (protected):
    Full CRUD. Only MANAGER can create, update, delete.

  Projects (protected):
    Full CRUD. Only MANAGER can create, update, delete.
    POST /api/projects/{id}/members — add a user to a project (MANAGER only)

  Tasks (protected):
    GET by project, GET by user, GET by status
    POST / (create) — MANAGER only
    POST /{id}/assign — assign to a user — MANAGER only
    PATCH /{id}/status — any authenticated user
    PUT /{id} — update task details — MANAGER only
    DELETE /{id} — MANAGER only

---

## AUTH & SECURITY

JWT: claims must include user ID, email, and role.
Token expiry, secret key, issuer, audience — all from appsettings.json. Do not hardcode.
Write a JWT helper class responsible for token generation and validation.
Write a middleware that extracts and validates the token on each request and sets HttpContext.User.
Use ASP.NET Core's built-in [Authorize] and [Authorize(Roles="...")] — do not reinvent authorization.

---

## ERROR HANDLING

Define custom exception types for: not found, conflict, business rule violation, unauthorized.
Write a global exception-handling middleware that catches these and maps them to appropriate HTTP status codes and the standard response envelope.
No try/catch in controllers or services except where unavoidable.

---

## UNIT TESTS

Test the Task service only. Cover these scenarios:
- Create task with valid project → succeeds
- Create task with non-existent project → throws correct exception
- Assign task to user not in project → throws correct exception
- Assign task to user in project → succeeds, repository updated
- Update status when already DONE → throws correct exception
- Update status with valid transition → succeeds

Mock all repository dependencies. No real database in tests.

---

## README

At solution root. Must include:
- How to start the SQL Server Docker container
- How to run the API (migrations run automatically)
- How to authenticate (register → login → use token)
- Role permissions summary

---

## ACCEPTANCE CRITERIA

Before finishing, verify:
1. `dotnet build` passes with zero errors and zero warnings if possible.
2. Every layer only depends on the layer directly below it.
3. No business logic outside the service layer.
4. No entity classes exposed in API responses.
5. All endpoints require authentication except /api/auth/*.
6. Enum values serialize as strings in JSON responses.
7. Unit tests: `dotnet test` passes with all 6 tests green.
8. Swagger UI accessible at /swagger and allows JWT token input.

Start by outputting the solution and project structure, then implement file by file.
State your pattern decisions once at the beginning — then apply them silently and consistently.