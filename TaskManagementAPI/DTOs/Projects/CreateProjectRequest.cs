using System.ComponentModel.DataAnnotations;

namespace TaskManagementAPI.DTOs.Projects;

public class CreateProjectRequest
{
    [Required]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    public Guid OwnerId { get; set; }
}
