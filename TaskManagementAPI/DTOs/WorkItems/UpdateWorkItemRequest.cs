using System.ComponentModel.DataAnnotations;
using TaskManagementAPI.DTOs.Validation;

namespace TaskManagementAPI.DTOs.WorkItems;

public class UpdateWorkItemRequest
{
    [Required]
    [StringLength(150)]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [FutureDate]
    public DateTime? Deadline { get; set; }
}
