using TaskManagementAPI.Domain.Entities;
using TaskManagementAPI.Domain.Enums;

namespace TaskManagementAPI.Services.Interfaces;

public interface IWorkItemService
{
    Task<List<WorkItem>> GetByProjectIdAsync(Guid projectId);
    Task<List<WorkItem>> GetByUserIdAsync(Guid userId);
    Task<List<WorkItem>> GetByStatusAsync(WorkItemStatus status);
    Task<WorkItem> CreateAsync(string title, string? description, DateTime? deadline, Guid projectId, Guid? assignedUserId);
    Task<WorkItem> AssignAsync(Guid workItemId, Guid userId);
    Task<WorkItem> UpdateStatusAsync(Guid workItemId, WorkItemStatus status);
    Task<WorkItem> UpdateAsync(Guid workItemId, string title, string? description, DateTime? deadline);
    Task DeleteAsync(Guid workItemId);
}
