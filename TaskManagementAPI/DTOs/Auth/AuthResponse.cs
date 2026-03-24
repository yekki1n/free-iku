using TaskManagementAPI.DTOs.Users;

namespace TaskManagementAPI.DTOs.Auth;

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public UserResponse User { get; set; } = new UserResponse();
}
