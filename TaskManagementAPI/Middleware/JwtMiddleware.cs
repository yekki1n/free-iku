using TaskManagementAPI.Exceptions;
using TaskManagementAPI.Helpers;

namespace TaskManagementAPI.Middleware;

public class JwtMiddleware
{
    private readonly RequestDelegate _next;

    public JwtMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IJwtTokenHelper jwtTokenHelper)
    {
        var authHeader = context.Request.Headers.Authorization.ToString();
        if (!string.IsNullOrWhiteSpace(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            var token = authHeader.Substring("Bearer ".Length).Trim();
            var principal = jwtTokenHelper.ValidateToken(token);
            if (principal == null)
            {
                throw new UnauthorizedException("Invalid or expired token.");
            }

            context.User = principal;
        }

        await _next(context);
    }
}
