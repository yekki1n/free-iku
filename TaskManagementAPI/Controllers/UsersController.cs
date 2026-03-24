using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagementAPI.DTOs.Users;
using TaskManagementAPI.Models;
using TaskManagementAPI.Services.Interfaces;

namespace TaskManagementAPI.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<UserResponse>>>> GetAll()
    {
        var users = await _userService.GetAllAsync();
        var response = users.Select(MapUser).ToList();
        return Ok(ApiResponse<List<UserResponse>>.Success(200, "Users retrieved.", response));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<UserResponse>>> GetById(Guid id)
    {
        var user = await _userService.GetByIdAsync(id);
        return Ok(ApiResponse<UserResponse>.Success(200, "User retrieved.", MapUser(user)));
    }

    [HttpPost]
    [Authorize(Roles = "MANAGER")]
    public async Task<ActionResult<ApiResponse<UserResponse>>> Create([FromBody] CreateUserRequest request)
    {
        var user = await _userService.CreateAsync(request.FullName, request.Email, request.Password, request.Role);
        var response = ApiResponse<UserResponse>.Success(201, "User created.", MapUser(user));
        return StatusCode(201, response);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "MANAGER")]
    public async Task<ActionResult<ApiResponse<UserResponse>>> Update(Guid id, [FromBody] UpdateUserRequest request)
    {
        var user = await _userService.UpdateAsync(id, request.FullName, request.Email, request.Role);
        return Ok(ApiResponse<UserResponse>.Success(200, "User updated.", MapUser(user)));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "MANAGER")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
    {
        await _userService.DeleteAsync(id);
        return Ok(ApiResponse<object>.Success(200, "User deleted.", null));
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
