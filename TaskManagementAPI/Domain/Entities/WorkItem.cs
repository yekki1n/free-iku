using TaskManagementAPI.Domain.Enums;

namespace TaskManagementAPI.Domain.Entities;

public class WorkItem
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public WorkItemStatus Status { get; set; } = WorkItemStatus.TODO;
    public DateTime? Deadline { get; set; }
    public Guid ProjectId { get; set; }
    public Guid? AssignedUserId { get; set; }

    public Project? Project { get; set; }
    public User? AssignedUser { get; set; }
}
