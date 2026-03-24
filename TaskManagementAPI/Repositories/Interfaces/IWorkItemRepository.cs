using TaskManagementAPI.Domain.Entities;
using TaskManagementAPI.Domain.Enums;

namespace TaskManagementAPI.Repositories.Interfaces;

public interface IWorkItemRepository
{
    Task<WorkItem?> GetByIdAsync(Guid id);
    Task<List<WorkItem>> GetByProjectIdAsync(Guid projectId);
    Task<List<WorkItem>> GetByUserIdAsync(Guid userId);
    Task<List<WorkItem>> GetByStatusAsync(WorkItemStatus status);
    Task AddAsync(WorkItem workItem);
    Task UpdateAsync(WorkItem workItem);
    Task DeleteAsync(WorkItem workItem);
}
