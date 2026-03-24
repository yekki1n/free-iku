using System.ComponentModel.DataAnnotations;

namespace TaskManagementAPI.DTOs.Projects;

public class AddProjectMemberRequest
{
    [Required]
    public Guid UserId { get; set; }
}
