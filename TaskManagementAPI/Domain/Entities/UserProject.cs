namespace TaskManagementAPI.Domain.Entities;

public class UserProject
{
    public Guid UserId { get; set; }
    public Guid ProjectId { get; set; }

    public User? User { get; set; }
    public Project? Project { get; set; }
}
