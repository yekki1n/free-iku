using TaskManagementAPI.Domain.Entities;
using TaskManagementAPI.Domain.Enums;

namespace TaskManagementAPI.Services.Interfaces;

public interface IUserService
{
    Task<List<User>> GetAllAsync();
    Task<User> GetByIdAsync(Guid id);
    Task<User> CreateAsync(string fullName, string email, string password, UserRole role);
    Task<User> UpdateAsync(Guid id, string fullName, string email, UserRole role);
    Task DeleteAsync(Guid id);
}
