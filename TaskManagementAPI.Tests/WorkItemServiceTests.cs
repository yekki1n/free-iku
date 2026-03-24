using Moq;
using TaskManagementAPI.Domain.Entities;
using TaskManagementAPI.Domain.Enums;
using TaskManagementAPI.Exceptions;
using TaskManagementAPI.Repositories.Interfaces;
using TaskManagementAPI.Services.Implementations;
using Xunit;

namespace TaskManagementAPI.Tests;

public class WorkItemServiceTests
{
    private readonly Mock<IWorkItemRepository> _workItemRepository = new();
    private readonly Mock<IProjectRepository> _projectRepository = new();
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IUserProjectRepository> _userProjectRepository = new();

    private WorkItemService CreateService()
    {
        return new WorkItemService(
            _workItemRepository.Object,
            _projectRepository.Object,
            _userRepository.Object,
            _userProjectRepository.Object);
    }

    [Fact]
    public async Task CreateTask_WithValidProject_Succeeds()
    {
        var projectId = Guid.NewGuid();
        _projectRepository.Setup(r => r.GetByIdAsync(projectId))
            .ReturnsAsync(new Project { Id = projectId });

        var service = CreateService();
        var result = await service.CreateAsync("Title", "Desc", DateTime.UtcNow.AddDays(1), projectId, null);

        Assert.Equal(projectId, result.ProjectId);
        _workItemRepository.Verify(r => r.AddAsync(It.IsAny<WorkItem>()), Times.Once);
    }

    [Fact]
    public async Task CreateTask_WithNonExistentProject_Throws()
    {
        var projectId = Guid.NewGuid();
        _projectRepository.Setup(r => r.GetByIdAsync(projectId))
            .ReturnsAsync((Project?)null);

        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.CreateAsync("Title", null, DateTime.UtcNow.AddDays(1), projectId, null));
    }

    [Fact]
    public async Task AssignTask_UserNotInProject_Throws()
    {
        var workItemId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _workItemRepository.Setup(r => r.GetByIdAsync(workItemId))
            .ReturnsAsync(new WorkItem { Id = workItemId, ProjectId = projectId });
        _userRepository.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(new User { Id = userId });
        _userProjectRepository.Setup(r => r.ExistsAsync(userId, projectId))
            .ReturnsAsync(false);

        var service = CreateService();

        await Assert.ThrowsAsync<BusinessRuleException>(() => service.AssignAsync(workItemId, userId));
    }

    [Fact]
    public async Task AssignTask_UserInProject_Succeeds()
    {
        var workItemId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _workItemRepository.Setup(r => r.GetByIdAsync(workItemId))
            .ReturnsAsync(new WorkItem { Id = workItemId, ProjectId = projectId });
        _userRepository.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(new User { Id = userId });
        _userProjectRepository.Setup(r => r.ExistsAsync(userId, projectId))
            .ReturnsAsync(true);

        var service = CreateService();
        var result = await service.AssignAsync(workItemId, userId);

        Assert.Equal(userId, result.AssignedUserId);
        _workItemRepository.Verify(r => r.UpdateAsync(It.IsAny<WorkItem>()), Times.Once);
    }

    [Fact]
    public async Task UpdateStatus_WhenDone_Throws()
    {
        var workItemId = Guid.NewGuid();
        _workItemRepository.Setup(r => r.GetByIdAsync(workItemId))
            .ReturnsAsync(new WorkItem { Id = workItemId, Status = WorkItemStatus.DONE });

        var service = CreateService();

        await Assert.ThrowsAsync<BusinessRuleException>(() => service.UpdateStatusAsync(workItemId, WorkItemStatus.IN_PROGRESS));
    }

    [Fact]
    public async Task UpdateStatus_WithValidTransition_Succeeds()
    {
        var workItemId = Guid.NewGuid();
        var item = new WorkItem { Id = workItemId, Status = WorkItemStatus.TODO };
        _workItemRepository.Setup(r => r.GetByIdAsync(workItemId))
            .ReturnsAsync(item);

        var service = CreateService();
        var result = await service.UpdateStatusAsync(workItemId, WorkItemStatus.IN_PROGRESS);

        Assert.Equal(WorkItemStatus.IN_PROGRESS, result.Status);
        _workItemRepository.Verify(r => r.UpdateAsync(It.IsAny<WorkItem>()), Times.Once);
    }
}
