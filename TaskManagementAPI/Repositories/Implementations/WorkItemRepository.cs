using Microsoft.EntityFrameworkCore;
using TaskManagementAPI.Data;
using TaskManagementAPI.Domain.Entities;
using TaskManagementAPI.Domain.Enums;
using TaskManagementAPI.Repositories.Interfaces;

namespace TaskManagementAPI.Repositories.Implementations;

public class WorkItemRepository : IWorkItemRepository
{
    private readonly AppDbContext _context;

    public WorkItemRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<WorkItem?> GetByIdAsync(Guid id)
    {
        return _context.WorkItems.FirstOrDefaultAsync(x => x.Id == id);
    }

    public Task<List<WorkItem>> GetByProjectIdAsync(Guid projectId)
    {
        return _context.WorkItems.AsNoTracking().Where(x => x.ProjectId == projectId).ToListAsync();
    }

    public Task<List<WorkItem>> GetByUserIdAsync(Guid userId)
    {
        return _context.WorkItems.AsNoTracking().Where(x => x.AssignedUserId == userId).ToListAsync();
    }

    public Task<List<WorkItem>> GetByStatusAsync(WorkItemStatus status)
    {
        return _context.WorkItems.AsNoTracking().Where(x => x.Status == status).ToListAsync();
    }

    public async Task AddAsync(WorkItem workItem)
    {
        _context.WorkItems.Add(workItem);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(WorkItem workItem)
    {
        _context.WorkItems.Update(workItem);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(WorkItem workItem)
    {
        _context.WorkItems.Remove(workItem);
        await _context.SaveChangesAsync();
    }
}
