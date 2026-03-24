using TaskManagementAPI.Domain.Enums;

namespace TaskManagementAPI.DTOs.WorkItems;

public class WorkItemResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public WorkItemStatus Status { get; set; }
    public DateTime? Deadline { get; set; }
    public Guid ProjectId { get; set; }
    public Guid? AssignedUserId { get; set; }
}
