using TaskManagementAPI.Domain.Entities;
using TaskManagementAPI.Domain.Enums;
using TaskManagementAPI.Exceptions;
using TaskManagementAPI.Repositories.Interfaces;
using TaskManagementAPI.Services.Interfaces;

namespace TaskManagementAPI.Services.Implementations;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserProjectRepository _userProjectRepository;

    public ProjectService(
        IProjectRepository projectRepository,
        IUserRepository userRepository,
        IUserProjectRepository userProjectRepository)
    {
        _projectRepository = projectRepository;
        _userRepository = userRepository;
        _userProjectRepository = userProjectRepository;
    }

    public Task<List<Project>> GetAllAsync()
    {
        return _projectRepository.GetAllAsync();
    }

    public async Task<Project> GetByIdAsync(Guid id)
    {
        var project = await _projectRepository.GetByIdAsync(id);
        if (project == null)
        {
            throw new NotFoundException("Project not found.");
        }

        return project;
    }

    public async Task<Project> CreateAsync(string name, string? description, Guid ownerId)
    {
        var owner = await _userRepository.GetByIdAsync(ownerId);
        if (owner == null)
        {
            throw new NotFoundException("Owner not found.");
        }

        if (owner.Role != UserRole.MANAGER)
        {
            throw new BusinessRuleException("Project owner must be a MANAGER.");
        }

        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            OwnerId = ownerId
        };

        await _projectRepository.AddAsync(project);

        var hasMembership = await _userProjectRepository.ExistsAsync(ownerId, project.Id);
        if (!hasMembership)
        {
            await _userProjectRepository.AddAsync(new UserProject
            {
                UserId = ownerId,
                ProjectId = project.Id
            });
        }

        return project;
    }

    public async Task<Project> UpdateAsync(Guid id, string name, string? description, Guid ownerId)
    {
        var project = await _projectRepository.GetByIdAsync(id);
        if (project == null)
        {
            throw new NotFoundException("Project not found.");
        }

        var owner = await _userRepository.GetByIdAsync(ownerId);
        if (owner == null)
        {
            throw new NotFoundException("Owner not found.");
        }

        if (owner.Role != UserRole.MANAGER)
        {
            throw new BusinessRuleException("Project owner must be a MANAGER.");
        }

        project.Name = name;
        project.Description = description;
        project.OwnerId = ownerId;

        await _projectRepository.UpdateAsync(project);

        var hasMembership = await _userProjectRepository.ExistsAsync(ownerId, project.Id);
        if (!hasMembership)
        {
            await _userProjectRepository.AddAsync(new UserProject
            {
                UserId = ownerId,
                ProjectId = project.Id
            });
        }

        return project;
    }

    public async Task DeleteAsync(Guid id)
    {
        var project = await _projectRepository.GetByIdAsync(id);
        if (project == null)
        {
            throw new NotFoundException("Project not found.");
        }

        await _projectRepository.DeleteAsync(project);
    }

    public async Task AddMemberAsync(Guid projectId, Guid userId)
    {
        var project = await _projectRepository.GetByIdAsync(projectId);
        if (project == null)
        {
            throw new NotFoundException("Project not found.");
        }

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new NotFoundException("User not found.");
        }

        var exists = await _userProjectRepository.ExistsAsync(userId, projectId);
        if (exists)
        {
            throw new ConflictException("User is already a member of this project.");
        }

        await _userProjectRepository.AddAsync(new UserProject
        {
            UserId = userId,
            ProjectId = projectId
        });
    }
}
