# TaskManagementAPI

## SQL Server (Docker)

```bash
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=@AAAaaa123123" -p 1433:1433 --name taskmanagement-sql -d mcr.microsoft.com/mssql/server:2022-latest
```

## Run The API

```bash
dotnet run --project TaskManagementAPI/TaskManagementAPI.csproj
```

Migrations run automatically on startup.

## Authentication Flow

1. Register: `POST /api/auth/register`
2. Login: `POST /api/auth/login` → returns JWT token
3. Use the token in Swagger or clients with header:

```
Authorization: Bearer {token}
```

## Seed Test Users

Seed runs on first API start if the database is empty. All seeded users share password `Password123!`.

- Managers
  - `ava.manager@local.test`
  - `noah.manager@local.test`
- Users
  - `liam.user@local.test`
  - `mia.user@local.test`
  - `ethan.user@local.test`

## Role Permissions Summary

- `MANAGER`
  - Users: create, update, delete
  - Projects: create, update, delete, add members
  - Tasks: create, assign, update, delete
- `USER`
  - Read users, projects, tasks
  - Update task status

All endpoints require authentication except `/api/auth/*`.

[Try more API test](./api-test.md)
