using System.ComponentModel.DataAnnotations;
using TaskManagementAPI.Domain.Enums;

namespace TaskManagementAPI.DTOs.WorkItems;

public class UpdateWorkItemStatusRequest
{
    [Required]
    public WorkItemStatus Status { get; set; }
}
