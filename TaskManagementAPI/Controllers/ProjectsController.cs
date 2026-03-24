using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagementAPI.DTOs.Projects;
using TaskManagementAPI.Models;
using TaskManagementAPI.Services.Interfaces;

namespace TaskManagementAPI.Controllers;

[ApiController]
[Route("api/projects")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectsController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<ProjectResponse>>>> GetAll()
    {
        var projects = await _projectService.GetAllAsync();
        var response = projects.Select(MapProject).ToList();
        return Ok(ApiResponse<List<ProjectResponse>>.Success(200, "Projects retrieved.", response));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ProjectResponse>>> GetById(Guid id)
    {
        var project = await _projectService.GetByIdAsync(id);
        return Ok(ApiResponse<ProjectResponse>.Success(200, "Project retrieved.", MapProject(project)));
    }

    [HttpPost]
    [Authorize(Roles = "MANAGER")]
    public async Task<ActionResult<ApiResponse<ProjectResponse>>> Create([FromBody] CreateProjectRequest request)
    {
        var project = await _projectService.CreateAsync(request.Name, request.Description, request.OwnerId);
        var response = ApiResponse<ProjectResponse>.Success(201, "Project created.", MapProject(project));
        return StatusCode(201, response);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "MANAGER")]
    public async Task<ActionResult<ApiResponse<ProjectResponse>>> Update(Guid id, [FromBody] UpdateProjectRequest request)
    {
        var project = await _projectService.UpdateAsync(id, request.Name, request.Description, request.OwnerId);
        return Ok(ApiResponse<ProjectResponse>.Success(200, "Project updated.", MapProject(project)));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "MANAGER")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
    {
        await _projectService.DeleteAsync(id);
        return Ok(ApiResponse<object>.Success(200, "Project deleted.", null));
    }

    [HttpPost("{id:guid}/members")]
    [Authorize(Roles = "MANAGER")]
    public async Task<ActionResult<ApiResponse<object>>> AddMember(Guid id, [FromBody] AddProjectMemberRequest request)
    {
        await _projectService.AddMemberAsync(id, request.UserId);
        return Ok(ApiResponse<object>.Success(200, "Member added.", null));
    }

    private static ProjectResponse MapProject(Domain.Entities.Project project)
    {
        return new ProjectResponse
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            OwnerId = project.OwnerId
        };
    }
}
