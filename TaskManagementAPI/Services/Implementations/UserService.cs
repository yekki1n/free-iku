using TaskManagementAPI.Domain.Entities;
using TaskManagementAPI.Domain.Enums;
using TaskManagementAPI.Exceptions;
using TaskManagementAPI.Repositories.Interfaces;
using TaskManagementAPI.Services.Interfaces;

namespace TaskManagementAPI.Services.Implementations;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public Task<List<User>> GetAllAsync()
    {
        return _userRepository.GetAllAsync();
    }

    public async Task<User> GetByIdAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            throw new NotFoundException("User not found.");
        }

        return user;
    }

    public async Task<User> CreateAsync(string fullName, string email, string password, UserRole role)
    {
        var existing = await _userRepository.GetByEmailAsync(email);
        if (existing != null)
        {
            throw new ConflictException("Email already exists.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = fullName,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = role
        };

        await _userRepository.AddAsync(user);
        return user;
    }

    public async Task<User> UpdateAsync(Guid id, string fullName, string email, UserRole role)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            throw new NotFoundException("User not found.");
        }

        var existing = await _userRepository.GetByEmailAsync(email);
        if (existing != null && existing.Id != id)
        {
            throw new ConflictException("Email already exists.");
        }

        user.FullName = fullName;
        user.Email = email;
        user.Role = role;

        await _userRepository.UpdateAsync(user);
        return user;
    }

    public async Task DeleteAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            throw new NotFoundException("User not found.");
        }

        await _userRepository.DeleteAsync(user);
    }
}
