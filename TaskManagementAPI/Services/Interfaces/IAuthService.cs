using TaskManagementAPI.Domain.Entities;

namespace TaskManagementAPI.Services.Interfaces;

public interface IAuthService
{
    Task<User> RegisterAsync(string fullName, string email, string password);
    Task<(string Token, User User)> LoginAsync(string email, string password);
}
