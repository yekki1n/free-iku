using System.ComponentModel.DataAnnotations;
using TaskManagementAPI.Domain.Enums;

namespace TaskManagementAPI.DTOs.Users;

public class UpdateUserRequest
{
    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public UserRole Role { get; set; }
}
