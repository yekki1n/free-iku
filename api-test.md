## Implementation Plan
Define reusable curl variables and tokens for MANAGER and USER to avoid repetition.
Cover all endpoints with both success and role-restriction cases, including rule violations.
Provide ID capture helpers so you can chain tests end-to-end.

# API Test (curl)

## Base

```bash
API_URL="http://localhost:5000"
CONTENT_TYPE="Content-Type: application/json"
```

## Auth (Public)

### Register (public)
```bash
curl -s -X POST "$API_URL/api/auth/register" \
  -H "$CONTENT_TYPE" \
  -d '{"fullName":"Test User","email":"test.user@local.test","password":"Password123!"}'
```

### Login (manager)
```bash
curl -s -X POST "$API_URL/api/auth/login" \
  -H "$CONTENT_TYPE" \
  -d '{"email":"ava.manager@local.test","password":"Password123!"}'
```

### Login (user)
```bash
curl -s -X POST "$API_URL/api/auth/login" \
  -H "$CONTENT_TYPE" \
  -d '{"email":"liam.user@local.test","password":"Password123!"}'
```

### Export tokens (requires jq)
```bash
MANAGER_TOKEN=$(curl -s -X POST "$API_URL/api/auth/login" -H "$CONTENT_TYPE" -d '{"email":"ava.manager@local.test","password":"Password123!"}' | jq -r '.data.token')
USER_TOKEN=$(curl -s -X POST "$API_URL/api/auth/login" -H "$CONTENT_TYPE" -d '{"email":"liam.user@local.test","password":"Password123!"}' | jq -r '.data.token')
```

## Users (Protected)

### GET all users (any authenticated)
```bash
curl -s -X GET "$API_URL/api/users" \
  -H "Authorization: Bearer $USER_TOKEN"
```

### GET user by id (any authenticated)
```bash
USER_ID=$(curl -s -X GET "$API_URL/api/users" -H "Authorization: Bearer $MANAGER_TOKEN" | jq -r '.data[0].id')

curl -s -X GET "$API_URL/api/users/$USER_ID" \
  -H "Authorization: Bearer $USER_TOKEN"
```

### CREATE user (manager only)
```bash
curl -s -X POST "$API_URL/api/users" \
  -H "$CONTENT_TYPE" \
  -H "Authorization: Bearer $MANAGER_TOKEN" \
  -d '{"fullName":"New User","email":"new.user@local.test","password":"Password123!","role":"USER"}'
```

### CREATE user (user forbidden)
```bash
curl -s -X POST "$API_URL/api/users" \
  -H "$CONTENT_TYPE" \
  -H "Authorization: Bearer $USER_TOKEN" \
  -d '{"fullName":"Should Fail","email":"fail.user@local.test","password":"Password123!","role":"USER"}'
```

### UPDATE user (manager only)
```bash
NEW_USER_ID=$(curl -s -X POST "$API_URL/api/users" -H "$CONTENT_TYPE" -H "Authorization: Bearer $MANAGER_TOKEN" -d '{"fullName":"Temp User","email":"temp.user@local.test","password":"Password123!","role":"USER"}' | jq -r '.data.id')

curl -s -X PUT "$API_URL/api/users/$NEW_USER_ID" \
  -H "$CONTENT_TYPE" \
  -H "Authorization: Bearer $MANAGER_TOKEN" \
  -d '{"fullName":"Temp User Updated","email":"temp.user@local.test","role":"USER"}'
```

### UPDATE user (user forbidden)
```bash
curl -s -X PUT "$API_URL/api/users/$NEW_USER_ID" \
  -H "$CONTENT_TYPE" \
  -H "Authorization: Bearer $USER_TOKEN" \
  -d '{"fullName":"Should Fail","email":"temp.user@local.test","role":"USER"}'
```

### DELETE user (manager only)
```bash
curl -s -X DELETE "$API_URL/api/users/$NEW_USER_ID" \
  -H "Authorization: Bearer $MANAGER_TOKEN"
```

### DELETE user (user forbidden)
```bash
curl -s -X DELETE "$API_URL/api/users/$USER_ID" \
  -H "Authorization: Bearer $USER_TOKEN"
```

## Projects (Protected)

### CREATE project (manager only)
```bash
MANAGER_ID=$(curl -s -X GET "$API_URL/api/users" -H "Authorization: Bearer $MANAGER_TOKEN" | jq -r '.data[] | select(.role=="MANAGER") | .id' | head -n 1)

PROJECT_ID=$(curl -s -X POST "$API_URL/api/projects" \
  -H "$CONTENT_TYPE" \
  -H "Authorization: Bearer $MANAGER_TOKEN" \
  -d "{\"name\":\"API Project\",\"description\":\"Test project\",\"ownerId\":\"$MANAGER_ID\"}" | jq -r '.data.id')
```

### CREATE project (user forbidden)
```bash
curl -s -X POST "$API_URL/api/projects" \
  -H "$CONTENT_TYPE" \
  -H "Authorization: Bearer $USER_TOKEN" \
  -d "{\"name\":\"Should Fail\",\"description\":\"Nope\",\"ownerId\":\"$MANAGER_ID\"}"
```

### GET all projects (any authenticated)
```bash
curl -s -X GET "$API_URL/api/projects" \
  -H "Authorization: Bearer $USER_TOKEN"
```

### GET project by id (any authenticated)
```bash
curl -s -X GET "$API_URL/api/projects/$PROJECT_ID" \
  -H "Authorization: Bearer $USER_TOKEN"
```

### UPDATE project (manager only)
```bash
curl -s -X PUT "$API_URL/api/projects/$PROJECT_ID" \
  -H "$CONTENT_TYPE" \
  -H "Authorization: Bearer $MANAGER_TOKEN" \
  -d "{\"name\":\"API Project Updated\",\"description\":\"Updated\",\"ownerId\":\"$MANAGER_ID\"}"
```

### UPDATE project (user forbidden)
```bash
curl -s -X PUT "$API_URL/api/projects/$PROJECT_ID" \
  -H "$CONTENT_TYPE" \
  -H "Authorization: Bearer $USER_TOKEN" \
  -d "{\"name\":\"Should Fail\",\"description\":\"Nope\",\"ownerId\":\"$MANAGER_ID\"}"
```

### ADD member (manager only)
```bash
curl -s -X POST "$API_URL/api/projects/$PROJECT_ID/members" \
  -H "$CONTENT_TYPE" \
  -H "Authorization: Bearer $MANAGER_TOKEN" \
  -d "{\"userId\":\"$USER_ID\"}"
```

### ADD member (user forbidden)
```bash
curl -s -X POST "$API_URL/api/projects/$PROJECT_ID/members" \
  -H "$CONTENT_TYPE" \
  -H "Authorization: Bearer $USER_TOKEN" \
  -d "{\"userId\":\"$USER_ID\"}"
```

### DELETE project (manager only)
```bash
curl -s -X DELETE "$API_URL/api/projects/$PROJECT_ID" \
  -H "Authorization: Bearer $MANAGER_TOKEN"
```

### DELETE project (user forbidden)
```bash
curl -s -X DELETE "$API_URL/api/projects/$PROJECT_ID" \
  -H "Authorization: Bearer $USER_TOKEN"
```

## Tasks (Protected)

### CREATE task (manager only)
```bash
PROJECT_ID=$(curl -s -X POST "$API_URL/api/projects" -H "$CONTENT_TYPE" -H "Authorization: Bearer $MANAGER_TOKEN" -d "{\"name\":\"Task Project\",\"description\":\"Task project\",\"ownerId\":\"$MANAGER_ID\"}" | jq -r '.data.id')

TASK_ID=$(curl -s -X POST "$API_URL/api/tasks" \
  -H "$CONTENT_TYPE" \
  -H "Authorization: Bearer $MANAGER_TOKEN" \
  -d "{\"title\":\"Task A\",\"description\":\"Demo\",\"deadline\":\"2030-01-01T00:00:00Z\",\"projectId\":\"$PROJECT_ID\"}" | jq -r '.data.id')
```

### CREATE task (user forbidden)
```bash
curl -s -X POST "$API_URL/api/tasks" \
  -H "$CONTENT_TYPE" \
  -H "Authorization: Bearer $USER_TOKEN" \
  -d "{\"title\":\"Should Fail\",\"projectId\":\"$PROJECT_ID\"}"
```

### CREATE task with past deadline (should fail)
```bash
curl -s -X POST "$API_URL/api/tasks" \
  -H "$CONTENT_TYPE" \
  -H "Authorization: Bearer $MANAGER_TOKEN" \
  -d "{\"title\":\"Bad Deadline\",\"deadline\":\"2000-01-01T00:00:00Z\",\"projectId\":\"$PROJECT_ID\"}"
```

### GET tasks by project (any authenticated)
```bash
curl -s -X GET "$API_URL/api/tasks/project/$PROJECT_ID" \
  -H "Authorization: Bearer $USER_TOKEN"
```

### GET tasks by user (any authenticated)
```bash
curl -s -X GET "$API_URL/api/tasks/user/$USER_ID" \
  -H "Authorization: Bearer $USER_TOKEN"
```

### GET tasks by status (any authenticated)
```bash
curl -s -X GET "$API_URL/api/tasks/status/TODO" \
  -H "Authorization: Bearer $USER_TOKEN"
```

### ASSIGN task to member (manager only)
```bash
curl -s -X POST "$API_URL/api/tasks/$TASK_ID/assign" \
  -H "$CONTENT_TYPE" \
  -H "Authorization: Bearer $MANAGER_TOKEN" \
  -d "{\"userId\":\"$USER_ID\"}"
```

### ASSIGN task by user (forbidden)
```bash
curl -s -X POST "$API_URL/api/tasks/$TASK_ID/assign" \
  -H "$CONTENT_TYPE" \
  -H "Authorization: Bearer $USER_TOKEN" \
  -d "{\"userId\":\"$USER_ID\"}"
```

### ASSIGN task to non-member (should fail)
```bash
NON_MEMBER_ID=$(curl -s -X POST "$API_URL/api/users" -H "$CONTENT_TYPE" -H "Authorization: Bearer $MANAGER_TOKEN" -d '{"fullName":"Outsider","email":"outsider@local.test","password":"Password123!","role":"USER"}' | jq -r '.data.id')

curl -s -X POST "$API_URL/api/tasks/$TASK_ID/assign" \
  -H "$CONTENT_TYPE" \
  -H "Authorization: Bearer $MANAGER_TOKEN" \
  -d "{\"userId\":\"$NON_MEMBER_ID\"}"
```

### UPDATE status (any authenticated)
```bash
curl -s -X PATCH "$API_URL/api/tasks/$TASK_ID/status" \
  -H "$CONTENT_TYPE" \
  -H "Authorization: Bearer $USER_TOKEN" \
  -d '{"status":"IN_PROGRESS"}'
```

### UPDATE status invalid transition (TODO -> DONE, should fail)
```bash
TASK_ID_2=$(curl -s -X POST "$API_URL/api/tasks" -H "$CONTENT_TYPE" -H "Authorization: Bearer $MANAGER_TOKEN" -d "{\"title\":\"Task B\",\"projectId\":\"$PROJECT_ID\"}" | jq -r '.data.id')

curl -s -X PATCH "$API_URL/api/tasks/$TASK_ID_2/status" \
  -H "$CONTENT_TYPE" \
  -H "Authorization: Bearer $USER_TOKEN" \
  -d '{"status":"DONE"}'
```

### UPDATE task details (manager only)
```bash
curl -s -X PUT "$API_URL/api/tasks/$TASK_ID" \
  -H "$CONTENT_TYPE" \
  -H "Authorization: Bearer $MANAGER_TOKEN" \
  -d '{"title":"Task A Updated","description":"Updated","deadline":"2031-01-01T00:00:00Z"}'
```

### UPDATE task details (user forbidden)
```bash
curl -s -X PUT "$API_URL/api/tasks/$TASK_ID" \
  -H "$CONTENT_TYPE" \
  -H "Authorization: Bearer $USER_TOKEN" \
  -d '{"title":"Should Fail","description":"Nope","deadline":"2031-01-01T00:00:00Z"}'
```

### DELETE task (manager only)
```bash
curl -s -X DELETE "$API_URL/api/tasks/$TASK_ID" \
  -H "Authorization: Bearer $MANAGER_TOKEN"
```

### DELETE task (user forbidden)
```bash
curl -s -X DELETE "$API_URL/api/tasks/$TASK_ID_2" \
  -H "Authorization: Bearer $USER_TOKEN"
```
