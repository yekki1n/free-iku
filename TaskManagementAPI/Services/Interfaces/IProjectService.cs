using TaskManagementAPI.Domain.Entities;

namespace TaskManagementAPI.Services.Interfaces;

public interface IProjectService
{
    Task<List<Project>> GetAllAsync();
    Task<Project> GetByIdAsync(Guid id);
    Task<Project> CreateAsync(string name, string? description, Guid ownerId);
    Task<Project> UpdateAsync(Guid id, string name, string? description, Guid ownerId);
    Task DeleteAsync(Guid id);
    Task AddMemberAsync(Guid projectId, Guid userId);
}
