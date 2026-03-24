using System.ComponentModel.DataAnnotations;

namespace TaskManagementAPI.DTOs.WorkItems;

public class AssignWorkItemRequest
{
    [Required]
    public Guid UserId { get; set; }
}
