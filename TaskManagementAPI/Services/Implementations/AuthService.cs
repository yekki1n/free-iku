using TaskManagementAPI.Domain.Entities;
using TaskManagementAPI.Domain.Enums;
using TaskManagementAPI.Exceptions;
using TaskManagementAPI.Helpers;
using TaskManagementAPI.Repositories.Interfaces;
using TaskManagementAPI.Services.Interfaces;

namespace TaskManagementAPI.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenHelper _jwtTokenHelper;

    public AuthService(IUserRepository userRepository, IJwtTokenHelper jwtTokenHelper)
    {
        _userRepository = userRepository;
        _jwtTokenHelper = jwtTokenHelper;
    }

    public async Task<User> RegisterAsync(string fullName, string email, string password)
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
            Role = UserRole.USER
        };

        await _userRepository.AddAsync(user);
        return user;
    }

    public async Task<(string Token, User User)> LoginAsync(string email, string password)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            throw new UnauthorizedException("Invalid credentials.");
        }

        var token = _jwtTokenHelper.GenerateToken(user);
        return (token, user);
    }
}
