using TaskManagementAPI.Domain.Entities;
using TaskManagementAPI.Domain.Enums;
using TaskManagementAPI.Exceptions;
using TaskManagementAPI.Repositories.Interfaces;
using TaskManagementAPI.Services.Interfaces;

namespace TaskManagementAPI.Services.Implementations;

public class WorkItemService : IWorkItemService
{
    private readonly IWorkItemRepository _workItemRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserProjectRepository _userProjectRepository;

    public WorkItemService(
        IWorkItemRepository workItemRepository,
        IProjectRepository projectRepository,
        IUserRepository userRepository,
        IUserProjectRepository userProjectRepository)
    {
        _workItemRepository = workItemRepository;
        _projectRepository = projectRepository;
        _userRepository = userRepository;
        _userProjectRepository = userProjectRepository;
    }

    public Task<List<WorkItem>> GetByProjectIdAsync(Guid projectId)
    {
        return _workItemRepository.GetByProjectIdAsync(projectId);
    }

    public Task<List<WorkItem>> GetByUserIdAsync(Guid userId)
    {
        return _workItemRepository.GetByUserIdAsync(userId);
    }

    public Task<List<WorkItem>> GetByStatusAsync(WorkItemStatus status)
    {
        return _workItemRepository.GetByStatusAsync(status);
    }

    public async Task<WorkItem> CreateAsync(string title, string? description, DateTime? deadline, Guid projectId, Guid? assignedUserId)
    {
        var project = await _projectRepository.GetByIdAsync(projectId);
        if (project == null)
        {
            throw new NotFoundException("Project not found.");
        }

        if (deadline.HasValue && deadline.Value <= DateTime.UtcNow)
        {
            throw new BusinessRuleException("Deadline must be a future date.");
        }

        if (assignedUserId.HasValue)
        {
            await EnsureUserInProjectAsync(assignedUserId.Value, projectId);
        }

        var workItem = new WorkItem
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = description,
            Deadline = deadline,
            ProjectId = projectId,
            AssignedUserId = assignedUserId,
            Status = WorkItemStatus.TODO
        };

        await _workItemRepository.AddAsync(workItem);
        return workItem;
    }

    public async Task<WorkItem> AssignAsync(Guid workItemId, Guid userId)
    {
        var workItem = await _workItemRepository.GetByIdAsync(workItemId);
        if (workItem == null)
        {
            throw new NotFoundException("Task not found.");
        }

        await EnsureUserInProjectAsync(userId, workItem.ProjectId);

        workItem.AssignedUserId = userId;
        await _workItemRepository.UpdateAsync(workItem);
        return workItem;
    }

    public async Task<WorkItem> UpdateStatusAsync(Guid workItemId, WorkItemStatus status)
    {
        var workItem = await _workItemRepository.GetByIdAsync(workItemId);
        if (workItem == null)
        {
            throw new NotFoundException("Task not found.");
        }

        if (workItem.Status == WorkItemStatus.DONE && status != WorkItemStatus.DONE)
        {
            throw new BusinessRuleException("Cannot change status after DONE.");
        }

        if (workItem.Status == WorkItemStatus.TODO && status == WorkItemStatus.DONE)
        {
            throw new BusinessRuleException("Invalid status transition.");
        }

        if (workItem.Status == WorkItemStatus.IN_PROGRESS && status == WorkItemStatus.TODO)
        {
            throw new BusinessRuleException("Invalid status transition.");
        }

        workItem.Status = status;
        await _workItemRepository.UpdateAsync(workItem);
        return workItem;
    }

    public async Task<WorkItem> UpdateAsync(Guid workItemId, string title, string? description, DateTime? deadline)
    {
        var workItem = await _workItemRepository.GetByIdAsync(workItemId);
        if (workItem == null)
        {
            throw new NotFoundException("Task not found.");
        }

        if (deadline.HasValue && deadline.Value <= DateTime.UtcNow)
        {
            throw new BusinessRuleException("Deadline must be a future date.");
        }

        workItem.Title = title;
        workItem.Description = description;
        workItem.Deadline = deadline;

        await _workItemRepository.UpdateAsync(workItem);
        return workItem;
    }

    public async Task DeleteAsync(Guid workItemId)
    {
        var workItem = await _workItemRepository.GetByIdAsync(workItemId);
        if (workItem == null)
        {
            throw new NotFoundException("Task not found.");
        }

        await _workItemRepository.DeleteAsync(workItem);
    }

    private async Task EnsureUserInProjectAsync(Guid userId, Guid projectId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new NotFoundException("User not found.");
        }

        var isMember = await _userProjectRepository.ExistsAsync(userId, projectId);
        if (!isMember)
        {
            throw new BusinessRuleException("User is not a member of the project.");
        }
    }
}
