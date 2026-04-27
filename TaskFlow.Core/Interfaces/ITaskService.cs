using TaskFlow.Core.DTOs;

namespace TaskFlow.Core.Interfaces;

public interface ITaskService
{
    Task<TaskItemDto> CreateAsync(Guid userId, CreateTaskRequest request);
    Task<IEnumerable<TaskItemDto>> GetAllAsync(Guid userId);
    Task<TaskItemDto> UpdateAsync(Guid userId, Guid taskId, UpdateTaskRequest request);
    Task DeleteAsync(Guid userId, Guid taskId);
}
