using SaasSystem.Enums;
using Volo.Abp.Application.Services;

namespace SaasSystem.Tasks;

public class KanbanColumnDto
{
    public WorkTaskStatus Status { get; set; }
    public string Label { get; set; } = string.Empty;
    public IReadOnlyList<KanbanTaskCardDto> Tasks { get; set; } = Array.Empty<KanbanTaskCardDto>();
}

public class KanbanTaskCardDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public int SortOrder { get; set; }
}

public class MoveTaskDto
{
    public Guid TaskId { get; set; }
    public WorkTaskStatus NewStatus { get; set; }
    public int NewOrder { get; set; }
}

public interface ITaskBoardAppService : IApplicationService
{
    Task<IReadOnlyList<KanbanColumnDto>> GetBoardAsync(Guid projectId);
    Task MoveAsync(MoveTaskDto input);
}
