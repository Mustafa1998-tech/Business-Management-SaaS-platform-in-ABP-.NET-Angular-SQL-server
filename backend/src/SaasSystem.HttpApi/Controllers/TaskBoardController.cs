using Microsoft.AspNetCore.Mvc;
using SaasSystem.Tasks;

namespace SaasSystem.Controllers;

[ApiController]
[Route("api/app/tasks")]
public class TaskBoardController : ControllerBase
{
    private readonly ITaskBoardAppService _taskBoardAppService;

    public TaskBoardController(ITaskBoardAppService taskBoardAppService)
    {
        _taskBoardAppService = taskBoardAppService;
    }

    [HttpGet("board/{projectId:guid}")]
    public async Task<IReadOnlyList<KanbanColumnDto>> GetBoardAsync(Guid projectId)
    {
        return await _taskBoardAppService.GetBoardAsync(projectId);
    }

    [HttpPost("move")]
    public async Task MoveAsync([FromBody] MoveTaskDto input)
    {
        await _taskBoardAppService.MoveAsync(input);
    }
}
