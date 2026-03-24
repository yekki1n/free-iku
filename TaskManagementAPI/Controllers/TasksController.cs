using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagementAPI.Domain.Enums;
using TaskManagementAPI.DTOs.WorkItems;
using TaskManagementAPI.Models;
using TaskManagementAPI.Services.Interfaces;

namespace TaskManagementAPI.Controllers;

[ApiController]
[Route("api/tasks")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly IWorkItemService _workItemService;

    public TasksController(IWorkItemService workItemService)
    {
        _workItemService = workItemService;
    }

    [HttpGet("project/{projectId:guid}")]
    public async Task<ActionResult<ApiResponse<List<WorkItemResponse>>>> GetByProject(Guid projectId)
    {
        var items = await _workItemService.GetByProjectIdAsync(projectId);
        var response = items.Select(MapWorkItem).ToList();
        return Ok(ApiResponse<List<WorkItemResponse>>.Success(200, "Tasks retrieved.", response));
    }

    [HttpGet("user/{userId:guid}")]
    public async Task<ActionResult<ApiResponse<List<WorkItemResponse>>>> GetByUser(Guid userId)
    {
        var items = await _workItemService.GetByUserIdAsync(userId);
        var response = items.Select(MapWorkItem).ToList();
        return Ok(ApiResponse<List<WorkItemResponse>>.Success(200, "Tasks retrieved.", response));
    }

    [HttpGet("status/{status}")]
    public async Task<ActionResult<ApiResponse<List<WorkItemResponse>>>> GetByStatus(WorkItemStatus status)
    {
        var items = await _workItemService.GetByStatusAsync(status);
        var response = items.Select(MapWorkItem).ToList();
        return Ok(ApiResponse<List<WorkItemResponse>>.Success(200, "Tasks retrieved.", response));
    }

    [HttpPost]
    [Authorize(Roles = "MANAGER")]
    public async Task<ActionResult<ApiResponse<WorkItemResponse>>> Create([FromBody] CreateWorkItemRequest request)
    {
        var item = await _workItemService.CreateAsync(
            request.Title,
            request.Description,
            request.Deadline,
            request.ProjectId,
            request.AssignedUserId);

        var response = ApiResponse<WorkItemResponse>.Success(201, "Task created.", MapWorkItem(item));
        return StatusCode(201, response);
    }

    [HttpPost("{id:guid}/assign")]
    [Authorize(Roles = "MANAGER")]
    public async Task<ActionResult<ApiResponse<WorkItemResponse>>> Assign(Guid id, [FromBody] AssignWorkItemRequest request)
    {
        var item = await _workItemService.AssignAsync(id, request.UserId);
        return Ok(ApiResponse<WorkItemResponse>.Success(200, "Task assigned.", MapWorkItem(item)));
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<ApiResponse<WorkItemResponse>>> UpdateStatus(Guid id, [FromBody] UpdateWorkItemStatusRequest request)
    {
        var item = await _workItemService.UpdateStatusAsync(id, request.Status);
        return Ok(ApiResponse<WorkItemResponse>.Success(200, "Status updated.", MapWorkItem(item)));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "MANAGER")]
    public async Task<ActionResult<ApiResponse<WorkItemResponse>>> Update(Guid id, [FromBody] UpdateWorkItemRequest request)
    {
        var item = await _workItemService.UpdateAsync(id, request.Title, request.Description, request.Deadline);
        return Ok(ApiResponse<WorkItemResponse>.Success(200, "Task updated.", MapWorkItem(item)));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "MANAGER")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
    {
        await _workItemService.DeleteAsync(id);
        return Ok(ApiResponse<object>.Success(200, "Task deleted.", null));
    }

    private static WorkItemResponse MapWorkItem(Domain.Entities.WorkItem item)
    {
        return new WorkItemResponse
        {
            Id = item.Id,
            Title = item.Title,
            Description = item.Description,
            Status = item.Status,
            Deadline = item.Deadline,
            ProjectId = item.ProjectId,
            AssignedUserId = item.AssignedUserId
        };
    }
}
