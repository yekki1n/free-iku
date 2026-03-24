using System.Security.Claims;
using TaskManagementAPI.Domain.Entities;

namespace TaskManagementAPI.Helpers;

public interface IJwtTokenHelper
{
    string GenerateToken(User user);
    ClaimsPrincipal? ValidateToken(string token);
}
