using TaskManagementAPI.Domain.Entities;
using TaskManagementAPI.Domain.Enums;

namespace TaskManagementAPI.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (context.Users.Any())
        {
            return;
        }

        var managers = new List<User>
        {
            new User
            {
                Id = Guid.NewGuid(),
                FullName = "Ava Manager",
                Email = "ava.manager@local.test",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                Role = UserRole.MANAGER
            },
            new User
            {
                Id = Guid.NewGuid(),
                FullName = "Noah Manager",
                Email = "noah.manager@local.test",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                Role = UserRole.MANAGER
            }
        };

        var users = new List<User>
        {
            new User
            {
                Id = Guid.NewGuid(),
                FullName = "Liam User",
                Email = "liam.user@local.test",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                Role = UserRole.USER
            },
            new User
            {
                Id = Guid.NewGuid(),
                FullName = "Mia User",
                Email = "mia.user@local.test",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                Role = UserRole.USER
            },
            new User
            {
                Id = Guid.NewGuid(),
                FullName = "Ethan User",
                Email = "ethan.user@local.test",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                Role = UserRole.USER
            }
        };

        context.Users.AddRange(managers);
        context.Users.AddRange(users);

        var projects = new List<Project>
        {
            new Project
            {
                Id = Guid.NewGuid(),
                Name = "Core Platform",
                Description = "Main platform deliverables",
                OwnerId = managers[0].Id
            },
            new Project
            {
                Id = Guid.NewGuid(),
                Name = "Mobile App",
                Description = "iOS and Android delivery",
                OwnerId = managers[0].Id
            },
            new Project
            {
                Id = Guid.NewGuid(),
                Name = "Data Pipeline",
                Description = "ETL modernization",
                OwnerId = managers[1].Id
            },
            new Project
            {
                Id = Guid.NewGuid(),
                Name = "Internal Tools",
                Description = "Operational tooling",
                OwnerId = managers[1].Id
            }
        };

        context.Projects.AddRange(projects);

        var userProjects = new List<UserProject>
        {
            new UserProject { UserId = managers[0].Id, ProjectId = projects[0].Id },
            new UserProject { UserId = managers[0].Id, ProjectId = projects[1].Id },
            new UserProject { UserId = managers[1].Id, ProjectId = projects[2].Id },
            new UserProject { UserId = managers[1].Id, ProjectId = projects[3].Id },
            new UserProject { UserId = users[0].Id, ProjectId = projects[0].Id },
            new UserProject { UserId = users[0].Id, ProjectId = projects[2].Id },
            new UserProject { UserId = users[1].Id, ProjectId = projects[1].Id },
            new UserProject { UserId = users[1].Id, ProjectId = projects[3].Id },
            new UserProject { UserId = users[2].Id, ProjectId = projects[0].Id },
            new UserProject { UserId = users[2].Id, ProjectId = projects[1].Id }
        };

        context.UserProjects.AddRange(userProjects);

        var allUsers = managers.Concat(users).ToList();
        var random = new Random(42);
        var workItems = new List<WorkItem>();

        for (var i = 0; i < 35; i++)
        {
            var project = projects[random.Next(projects.Count)];
            var status = (WorkItemStatus)(i % 3);
            var deadline = DateTime.UtcNow.AddDays(7 + i);

            var eligibleUsers = userProjects
                .Where(x => x.ProjectId == project.Id)
                .Select(x => allUsers.First(u => u.Id == x.UserId))
                .ToList();

            var assign = random.Next(0, 2) == 1;
            var assignedUser = assign ? eligibleUsers[random.Next(eligibleUsers.Count)] : null;

            workItems.Add(new WorkItem
            {
                Id = Guid.NewGuid(),
                Title = $"Task {i + 1}",
                Description = "Seeded work item",
                Status = status,
                Deadline = deadline,
                ProjectId = project.Id,
                AssignedUserId = assignedUser?.Id
            });
        }

        context.WorkItems.AddRange(workItems);

        await context.SaveChangesAsync();
    }
}
