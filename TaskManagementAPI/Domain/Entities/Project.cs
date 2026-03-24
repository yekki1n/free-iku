namespace TaskManagementAPI.Domain.Entities;

public class Project
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid OwnerId { get; set; }

    public User? Owner { get; set; }
    public ICollection<UserProject> UserProjects { get; set; } = new List<UserProject>();
    public ICollection<WorkItem> WorkItems { get; set; } = new List<WorkItem>();
}
