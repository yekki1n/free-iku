using Microsoft.EntityFrameworkCore;
using TaskManagementAPI.Data;
using TaskManagementAPI.Domain.Entities;
using TaskManagementAPI.Repositories.Interfaces;

namespace TaskManagementAPI.Repositories.Implementations;

public class UserProjectRepository : IUserProjectRepository
{
    private readonly AppDbContext _context;

    public UserProjectRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(UserProject userProject)
    {
        _context.UserProjects.Add(userProject);
        await _context.SaveChangesAsync();
    }

    public Task<bool> ExistsAsync(Guid userId, Guid projectId)
    {
        return _context.UserProjects.AnyAsync(x => x.UserId == userId && x.ProjectId == projectId);
    }

    public Task<List<Guid>> GetUserIdsByProjectAsync(Guid projectId)
    {
        return _context.UserProjects
            .Where(x => x.ProjectId == projectId)
            .Select(x => x.UserId)
            .ToListAsync();
    }
}
