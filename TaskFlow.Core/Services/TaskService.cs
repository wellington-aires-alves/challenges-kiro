using TaskFlow.Core.DTOs;
using TaskFlow.Core.Entities;
using TaskFlow.Core.Interfaces;

namespace TaskFlow.Core.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;

    public TaskService(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<TaskItemDto> CreateAsync(Guid userId, CreateTaskRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ValidationException("Título é obrigatório.");

        if (request.Title.Length > 200)
            throw new ValidationException("O título deve ter no máximo 200 caracteres.");

        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = request.Title,
            Description = request.Description,
            Status = Entities.TaskStatus.Pendente,
            CreatedAt = DateTime.UtcNow
        };

        await _taskRepository.AddAsync(task);

        return MapToDto(task);
    }

    public async Task<IEnumerable<TaskItemDto>> GetAllAsync(Guid userId)
    {
        var tasks = await _taskRepository.GetByUserIdAsync(userId);

        return tasks
            .OrderByDescending(t => t.CreatedAt)
            .Select(MapToDto);
    }

    public async Task<TaskItemDto> UpdateAsync(Guid userId, Guid taskId, UpdateTaskRequest request)
    {
        var task = await _taskRepository.GetByIdAsync(taskId);

        if (task is null)
            throw new NotFoundException("Tarefa não encontrada.");

        if (task.UserId != userId)
            throw new ForbiddenException("Acesso negado.");

        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ValidationException("Título é obrigatório.");

        if (request.Title.Length > 200)
            throw new ValidationException("O título deve ter no máximo 200 caracteres.");

        if (!Enum.TryParse<Entities.TaskStatus>(request.Status, out var status))
            throw new ValidationException("Status inválido. Valores aceitos: Pendente, Concluida.");

        task.Title = request.Title;
        task.Description = request.Description;
        task.Status = status;

        await _taskRepository.UpdateAsync(task);

        return MapToDto(task);
    }

    public async Task DeleteAsync(Guid userId, Guid taskId)
    {
        var task = await _taskRepository.GetByIdAsync(taskId);

        if (task is null)
            throw new NotFoundException("Tarefa não encontrada.");

        if (task.UserId != userId)
            throw new ForbiddenException("Acesso negado.");

        await _taskRepository.DeleteAsync(taskId);
    }

    private static TaskItemDto MapToDto(TaskItem task) =>
        new(task.Id, task.Title, task.Description, task.Status.ToString(), task.CreatedAt);
}
