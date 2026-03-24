using TaskManagementAPI.Domain.Entities;

namespace TaskManagementAPI.Repositories.Interfaces;

public interface IUserProjectRepository
{
    Task AddAsync(UserProject userProject);
    Task<bool> ExistsAsync(Guid userId, Guid projectId);
    Task<List<Guid>> GetUserIdsByProjectAsync(Guid projectId);
}
