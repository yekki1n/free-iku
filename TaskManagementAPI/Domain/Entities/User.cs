using TaskManagementAPI.Domain.Enums;

namespace TaskManagementAPI.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }

    public ICollection<Project> OwnedProjects { get; set; } = new List<Project>();
    public ICollection<UserProject> UserProjects { get; set; } = new List<UserProject>();
    public ICollection<WorkItem> AssignedWorkItems { get; set; } = new List<WorkItem>();
}
