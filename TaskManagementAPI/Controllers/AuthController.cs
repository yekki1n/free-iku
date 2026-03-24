using Microsoft.AspNetCore.Mvc;
using TaskManagementAPI.DTOs.Auth;
using TaskManagementAPI.DTOs.Users;
using TaskManagementAPI.Models;
using TaskManagementAPI.Services.Interfaces;

namespace TaskManagementAPI.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<UserResponse>>> Register([FromBody] RegisterRequest request)
    {
        var user = await _authService.RegisterAsync(request.FullName, request.Email, request.Password);
        var response = ApiResponse<UserResponse>.Success(201, "User registered.", MapUser(user));
        return StatusCode(201, response);
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request.Email, request.Password);
        var response = new AuthResponse
        {
            Token = result.Token,
            User = MapUser(result.User)
        };

        return Ok(ApiResponse<AuthResponse>.Success(200, "Login successful.", response));
    }

    private static UserResponse MapUser(Domain.Entities.User user)
    {
        return new UserResponse
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role
        };
    }
}
