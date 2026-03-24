const API_BASE = "http://localhost:5000";

const STORAGE_TOKEN_KEY = "tm_token";
const STORAGE_USER_KEY = "tm_user";

function setLoading(isLoading) {
  const el = document.getElementById("loading");
  if (!el) return;
  el.textContent = isLoading ? "Loading..." : "";
}

function setError(message) {
  const el = document.getElementById("error");
  if (!el) return;
  el.textContent = message || "";
}

function setMessage(message) {
  const el = document.getElementById("message");
  if (!el) return;
  el.textContent = message || "";
}

function getToken() {
  return localStorage.getItem(STORAGE_TOKEN_KEY);
}

function getUser() {
  const raw = localStorage.getItem(STORAGE_USER_KEY);
  return raw ? JSON.parse(raw) : null;
}

function setAuth(token, user) {
  localStorage.setItem(STORAGE_TOKEN_KEY, token);
  localStorage.setItem(STORAGE_USER_KEY, JSON.stringify(user));
}

function clearAuth() {
  localStorage.removeItem(STORAGE_TOKEN_KEY);
  localStorage.removeItem(STORAGE_USER_KEY);
}

function requireAuth() {
  const token = getToken();
  if (!token) {
    window.location.href = "login.html";
    return false;
  }
  return true;
}

async function apiFetch(path, options) {
  setLoading(true);
  try {
    const token = getToken();
    const opts = options || {};
    const headers = opts.headers ? { ...opts.headers } : {};
    if (opts.body && !headers["Content-Type"]) {
      headers["Content-Type"] = "application/json";
    }
    if (token) {
      headers.Authorization = "Bearer " + token;
    }

    const response = await fetch(API_BASE + path, {
      ...opts,
      headers,
    });

    if (response.status === 401) {
      clearAuth();
      window.location.href = "login.html";
      return null;
    }

    if (response.status === 403) {
      return { error: "You do not have permission to perform this action." };
    }

    const data = await response.json();
    if (!response.ok) {
      return { error: data.message || "Request failed." };
    }

    return data;
  } catch (err) {
    return { error: "Network error." };
  } finally {
    setLoading(false);
  }
}

function renderNav() {
  const nav = document.getElementById("top-nav");
  if (!nav) return;

  const user = getUser();
  const role = user ? user.role : "";
  const name = user ? user.fullName : "";

  const usersLink = role === "MANAGER" ? '<a href="users.html">Users</a>' : "";

  nav.innerHTML =
    '<table width="100%" cellspacing="0" cellpadding="4" border="1">' +
    "<tr>" +
    '<td><a href="login.html">Login</a></td>' +
    '<td><a href="register.html">Register</a></td>' +
    '<td><a href="dashboard.html">Dashboard</a></td>' +
    (usersLink ? '<td>' + usersLink + "</td>" : "") +
    '<td><a href="projects.html">Projects</a></td>' +
    '<td><a href="tasks.html">Tasks</a></td>' +
    '<td><a href="#" id="logout-link">Logout</a></td>' +
    '<td align="right">' +
    (user ? "Logged in: " + name + " [" + role + "]" : "Not logged in") +
    "</td>" +
    "</tr>" +
    "</table>";

  const logoutLink = document.getElementById("logout-link");
  if (logoutLink) {
    logoutLink.addEventListener("click", function (e) {
      e.preventDefault();
      clearAuth();
      window.location.href = "login.html";
    });
  }
}

function getQueryParam(name) {
  const params = new URLSearchParams(window.location.search);
  return params.get(name);
}

function initIndex() {
  const token = getToken();
  if (token) {
    window.location.href = "dashboard.html";
  } else {
    window.location.href = "login.html";
  }
}

function initLogin() {
  renderNav();

  const msg = getQueryParam("msg");
  if (msg) {
    setMessage(msg);
  }

  const form = document.getElementById("login-form");
  form.addEventListener("submit", async function (e) {
    e.preventDefault();
    setError("");
    setMessage("");

    const email = document.getElementById("login-email").value;
    const password = document.getElementById("login-password").value;

    const result = await apiFetch("/api/auth/login", {
      method: "POST",
      body: JSON.stringify({ email, password }),
    });

    if (!result || result.error) {
      setError(result ? result.error : "Login failed.");
      return;
    }

    setAuth(result.data.token, result.data.user);
    window.location.href = "dashboard.html";
  });
}

function initRegister() {
  renderNav();

  const form = document.getElementById("register-form");
  form.addEventListener("submit", async function (e) {
    e.preventDefault();
    setError("");

    const fullName = document.getElementById("register-fullname").value;
    const email = document.getElementById("register-email").value;
    const password = document.getElementById("register-password").value;
    const role = document.getElementById("register-role").value;

    const result = await apiFetch("/api/auth/register", {
      method: "POST",
      body: JSON.stringify({ fullName, email, password, role }),
    });

    if (!result || result.error) {
      setError(result ? result.error : "Registration failed.");
      return;
    }

    window.location.href = "login.html?msg=" + encodeURIComponent("Registration successful. Please log in.");
  });
}

async function initDashboard() {
  if (!requireAuth()) return;
  renderNav();

  const user = getUser();
  const welcome = document.getElementById("welcome");
  welcome.textContent = "Welcome, " + user.fullName + " — Role: " + user.role;

  const usersResult = await apiFetch("/api/users");
  const projectsResult = await apiFetch("/api/projects");

  const todoResult = await apiFetch("/api/tasks/status/TODO");
  const inProgressResult = await apiFetch("/api/tasks/status/IN_PROGRESS");
  const doneResult = await apiFetch("/api/tasks/status/DONE");

  if (usersResult && projectsResult && todoResult && inProgressResult && doneResult) {
    const totalTasks =
      (todoResult.data ? todoResult.data.length : 0) +
      (inProgressResult.data ? inProgressResult.data.length : 0) +
      (doneResult.data ? doneResult.data.length : 0);

    document.getElementById("total-users").textContent = usersResult.data.length;
    document.getElementById("total-projects").textContent = projectsResult.data.length;
    document.getElementById("total-tasks").textContent = totalTasks;
  }

  const projectMap = {};
  if (projectsResult && projectsResult.data) {
    projectsResult.data.forEach(function (p) {
      projectMap[p.id] = p.name;
    });
  }

  const tasksResult = await apiFetch("/api/tasks/user/" + user.id);
  if (tasksResult && tasksResult.data) {
    const tbody = document.getElementById("my-tasks-body");
    tbody.innerHTML = "";

    tasksResult.data.forEach(function (t) {
      const row = document.createElement("tr");
      row.innerHTML =
        "<td>" + t.title + "</td>" +
        "<td>" + (projectMap[t.projectId] || t.projectId) + "</td>" +
        "<td>[" + t.status + "]</td>" +
        "<td>" + (t.deadline || "") + "</td>";
      tbody.appendChild(row);
    });
  }
}

async function initUsers() {
  if (!requireAuth()) return;
  renderNav();

  const user = getUser();
  if (user.role !== "MANAGER") {
    window.location.href = "dashboard.html";
    return;
  }

  const result = await apiFetch("/api/users");
  if (result && result.data) {
    const tbody = document.getElementById("users-body");
    tbody.innerHTML = "";

    result.data.forEach(function (u) {
      const row = document.createElement("tr");
      row.innerHTML =
        "<td>" + u.id + "</td>" +
        "<td>" + u.fullName + "</td>" +
        "<td>" + u.email + "</td>" +
        "<td>[" + u.role + "]</td>" +
        "<td><button type=\"button\" data-id=\"" + u.id + "\">Delete</button></td>";
      tbody.appendChild(row);
    });

    tbody.querySelectorAll("button").forEach(function (btn) {
      btn.addEventListener("click", async function () {
        const id = btn.getAttribute("data-id");
        if (!confirm("Delete this user?")) return;
        const delResult = await apiFetch("/api/users/" + id, { method: "DELETE" });
        if (delResult && delResult.error) {
          setError(delResult.error);
          return;
        }
        window.location.reload();
      });
    });
  }

  const form = document.getElementById("create-user-form");
  form.addEventListener("submit", async function (e) {
    e.preventDefault();
    setError("");

    const fullName = document.getElementById("create-user-fullname").value;
    const email = document.getElementById("create-user-email").value;
    const password = document.getElementById("create-user-password").value;
    const role = document.getElementById("create-user-role").value;

    const resultCreate = await apiFetch("/api/users", {
      method: "POST",
      body: JSON.stringify({ fullName, email, password, role }),
    });

    if (!resultCreate || resultCreate.error) {
      setError(resultCreate ? resultCreate.error : "Create failed.");
      return;
    }

    window.location.reload();
  });
}

async function initProjects() {
  if (!requireAuth()) return;
  renderNav();

  const currentUser = getUser();
  const isManager = currentUser.role === "MANAGER";

  const usersResult = await apiFetch("/api/users");
  const projectsResult = await apiFetch("/api/projects");

  const userMap = {};
  if (usersResult && usersResult.data) {
    usersResult.data.forEach(function (u) {
      userMap[u.id] = u.fullName;
    });
  }

  if (projectsResult && projectsResult.data) {
    const tbody = document.getElementById("projects-body");
    tbody.innerHTML = "";

    projectsResult.data.forEach(function (p) {
      const row = document.createElement("tr");
      const ownerName = userMap[p.ownerId] || p.ownerId;
      row.innerHTML =
        "<td>" + p.id + "</td>" +
        "<td>" + p.name + "</td>" +
        "<td>" + (p.description || "") + "</td>" +
        "<td>" + ownerName + "</td>" +
        "<td><a href=\"tasks.html?projectId=" + p.id + "\">View Tasks</a></td>" +
        (isManager ? "<td><button type=\"button\" data-id=\"" + p.id + "\">Delete</button></td>" : "");
      tbody.appendChild(row);
    });

    if (isManager) {
      tbody.querySelectorAll("button").forEach(function (btn) {
        btn.addEventListener("click", async function () {
          const id = btn.getAttribute("data-id");
          if (!confirm("Delete this project?")) return;
          const delResult = await apiFetch("/api/projects/" + id, { method: "DELETE" });
          if (delResult && delResult.error) {
            setError(delResult.error);
            return;
          }
          window.location.reload();
        });
      });
    }
  }

  const createForm = document.getElementById("create-project-form");
  const addMemberForm = document.getElementById("add-member-form");
  const createFieldset = createForm ? createForm.parentElement : null;
  const addMemberFieldset = addMemberForm ? addMemberForm.parentElement : null;

  if (isManager) {
    createForm.addEventListener("submit", async function (e) {
      e.preventDefault();
      setError("");

      const name = document.getElementById("create-project-name").value;
      const description = document.getElementById("create-project-description").value;
      const ownerId = document.getElementById("create-project-owner").value;

      const resultCreate = await apiFetch("/api/projects", {
        method: "POST",
        body: JSON.stringify({ name, description, ownerId }),
      });

      if (!resultCreate || resultCreate.error) {
        setError(resultCreate ? resultCreate.error : "Create failed.");
        return;
      }

      window.location.reload();
    });

    addMemberForm.addEventListener("submit", async function (e) {
      e.preventDefault();
      setError("");

      const projectId = document.getElementById("add-member-project").value;
      const userId = document.getElementById("add-member-user").value;

      const resultAdd = await apiFetch("/api/projects/" + projectId + "/members", {
        method: "POST",
        body: JSON.stringify({ userId }),
      });

      if (!resultAdd || resultAdd.error) {
        setError(resultAdd ? resultAdd.error : "Add failed.");
        return;
      }

      window.location.reload();
    });
  } else {
    if (createFieldset) createFieldset.style.display = "none";
    if (addMemberFieldset) addMemberFieldset.style.display = "none";
  }
}

async function initTasks() {
  if (!requireAuth()) return;
  renderNav();

  const currentUser = getUser();
  const isManager = currentUser.role === "MANAGER";

  const usersResult = await apiFetch("/api/users");
  const projectsResult = await apiFetch("/api/projects");

  const userMap = {};
  if (usersResult && usersResult.data) {
    usersResult.data.forEach(function (u) {
      userMap[u.id] = u.fullName;
    });
  }

  const projectMap = {};
  if (projectsResult && projectsResult.data) {
    projectsResult.data.forEach(function (p) {
      projectMap[p.id] = p.name;
    });
  }

  async function loadTasks(filter) {
    setError("");
    let endpoint = "/api/tasks/user/" + currentUser.id;

    if (filter.projectId) {
      endpoint = "/api/tasks/project/" + filter.projectId;
    } else if (filter.status) {
      endpoint = "/api/tasks/status/" + filter.status;
    } else if (filter.userId) {
      endpoint = "/api/tasks/user/" + filter.userId;
    }

    const result = await apiFetch(endpoint);
    if (!result || result.error) {
      setError(result ? result.error : "Load failed.");
      return;
    }

    const tbody = document.getElementById("tasks-body");
    tbody.innerHTML = "";

    result.data.forEach(function (t) {
      const row = document.createElement("tr");
      const assigned = t.assignedUserId ? (userMap[t.assignedUserId] || t.assignedUserId) : "";
      const project = projectMap[t.projectId] || t.projectId;
      const statusSelect =
        '<select data-id="' + t.id + '">' +
        '<option value="TODO"' + (t.status === "TODO" ? " selected" : "") + '>TODO</option>' +
        '<option value="IN_PROGRESS"' + (t.status === "IN_PROGRESS" ? " selected" : "") + '>IN_PROGRESS</option>' +
        '<option value="DONE"' + (t.status === "DONE" ? " selected" : "") + '>DONE</option>' +
        "</select>";

      row.innerHTML =
        "<td>" + t.id + "</td>" +
        "<td>" + t.title + "</td>" +
        "<td>" + project + "</td>" +
        "<td>" + assigned + "</td>" +
        "<td>[" + t.status + "]</td>" +
        "<td>" + (t.deadline || "") + "</td>" +
        "<td>" + statusSelect + " <button type=\"button\" data-action=\"update\" data-id=\"" + t.id + "\">Update</button></td>";

      tbody.appendChild(row);
    });

    tbody.querySelectorAll("button[data-action='update']").forEach(function (btn) {
      btn.addEventListener("click", async function () {
        const id = btn.getAttribute("data-id");
        const select = tbody.querySelector("select[data-id='" + id + "']");
        const status = select.value;

        const upd = await apiFetch("/api/tasks/" + id + "/status", {
          method: "PATCH",
          body: JSON.stringify({ status }),
        });

        if (!upd || upd.error) {
          setError(upd ? upd.error : "Update failed.");
          return;
        }

        await loadTasks({});
      });
    });
  }

  const projectIdParam = getQueryParam("projectId");
  if (projectIdParam) {
    document.getElementById("filter-project-id").value = projectIdParam;
    await loadTasks({ projectId: projectIdParam });
  } else {
    await loadTasks({});
  }

  const filterForm = document.getElementById("filter-form");
  filterForm.addEventListener("submit", async function (e) {
    e.preventDefault();
    const projectId = document.getElementById("filter-project-id").value;
    const status = document.getElementById("filter-status").value;
    const userId = document.getElementById("filter-user-id").value;
    await loadTasks({ projectId, status, userId });
  });

  const createTaskForm = document.getElementById("create-task-form");
  const assignTaskForm = document.getElementById("assign-task-form");
  const createTaskFieldset = createTaskForm ? createTaskForm.parentElement : null;
  const assignTaskFieldset = assignTaskForm ? assignTaskForm.parentElement : null;

  if (isManager) {
    createTaskForm.addEventListener("submit", async function (e) {
      e.preventDefault();
      setError("");

      const title = document.getElementById("create-task-title").value;
      const description = document.getElementById("create-task-description").value;
      const deadline = document.getElementById("create-task-deadline").value;
      const projectId = document.getElementById("create-task-project").value;

      const payload = { title, description, projectId };
      if (deadline) {
        payload.deadline = deadline + "T00:00:00Z";
      }

      const resultCreate = await apiFetch("/api/tasks", {
        method: "POST",
        body: JSON.stringify(payload),
      });

      if (!resultCreate || resultCreate.error) {
        setError(resultCreate ? resultCreate.error : "Create failed.");
        return;
      }

      window.location.reload();
    });

    assignTaskForm.addEventListener("submit", async function (e) {
      e.preventDefault();
      setError("");

      const taskId = document.getElementById("assign-task-id").value;
      const userId = document.getElementById("assign-task-user").value;

      const resultAssign = await apiFetch("/api/tasks/" + taskId + "/assign", {
        method: "POST",
        body: JSON.stringify({ userId }),
      });

      if (!resultAssign || resultAssign.error) {
        setError(resultAssign ? resultAssign.error : "Assign failed.");
        return;
      }

      window.location.reload();
    });
  } else {
    if (createTaskFieldset) createTaskFieldset.style.display = "none";
    if (assignTaskFieldset) assignTaskFieldset.style.display = "none";
  }
}

document.addEventListener("DOMContentLoaded", function () {
  const page = document.body.getAttribute("data-page");

  if (page === "index") initIndex();
  if (page === "login") initLogin();
  if (page === "register") initRegister();
  if (page === "dashboard") initDashboard();
  if (page === "users") initUsers();
  if (page === "projects") initProjects();
  if (page === "tasks") initTasks();
});
