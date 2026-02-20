using Microsoft.AspNetCore.Authorization;
using SaasSystem.Domain.Entities;
using SaasSystem.Enums;
using SaasSystem.Permissions;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SaasSystem.Tasks;

[Authorize(SaasSystemPermissions.Tasks.Default)]
public class TaskBoardAppService : ApplicationService, ITaskBoardAppService
{
    private readonly IRepository<WorkTask, Guid> _taskRepository;

    public TaskBoardAppService(IRepository<WorkTask, Guid> taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<IReadOnlyList<KanbanColumnDto>> GetBoardAsync(Guid projectId)
    {
        IQueryable<WorkTask> queryable = await _taskRepository.GetQueryableAsync();
        List<WorkTask> tasks = await AsyncExecuter.ToListAsync(
            queryable
                .Where(x => x.ProjectId == projectId)
                .OrderBy(x => x.Status)
                .ThenBy(x => x.SortOrder));

        Dictionary<WorkTaskStatus, List<WorkTask>> grouped = tasks
            .GroupBy(x => x.Status)
            .ToDictionary(x => x.Key, x => x.ToList());

        List<KanbanColumnDto> columns = Enum.GetValues<WorkTaskStatus>()
            .Select(status => new KanbanColumnDto
            {
                Status = status,
                Label = status.ToString(),
                Tasks = grouped.TryGetValue(status, out List<WorkTask>? list)
                    ? ObjectMapper.Map<List<WorkTask>, List<KanbanTaskCardDto>>(list)
                    : new List<KanbanTaskCardDto>()
            })
            .ToList();

        return columns;
    }

    [Authorize(SaasSystemPermissions.Tasks.Move)]
    public async Task MoveAsync(MoveTaskDto input)
    {
        WorkTask entity = await _taskRepository.GetAsync(input.TaskId);
        entity.MoveTo(input.NewStatus, input.NewOrder);
        await _taskRepository.UpdateAsync(entity, autoSave: true);
    }
}
