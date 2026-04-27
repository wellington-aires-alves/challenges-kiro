using TaskFlow.Core.Entities;
using TaskFlow.Core.Interfaces;

namespace TaskFlow.API;

public class InMemoryTaskRepository : ITaskRepository
{
    private readonly List<TaskItem> _tasks = new();
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public async Task<TaskItem?> GetByIdAsync(Guid id)
    {
        await _semaphore.WaitAsync();
        try
        {
            return _tasks.FirstOrDefault(t => t.Id == id);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<IEnumerable<TaskItem>> GetByUserIdAsync(Guid userId)
    {
        await _semaphore.WaitAsync();
        try
        {
            return _tasks.Where(t => t.UserId == userId).ToList();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task AddAsync(TaskItem task)
    {
        await _semaphore.WaitAsync();
        try
        {
            _tasks.Add(task);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task UpdateAsync(TaskItem task)
    {
        await _semaphore.WaitAsync();
        try
        {
            var index = _tasks.FindIndex(t => t.Id == task.Id);
            if (index >= 0)
            {
                _tasks[index] = task;
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        await _semaphore.WaitAsync();
        try
        {
            var task = _tasks.FirstOrDefault(t => t.Id == id);
            if (task is not null)
            {
                _tasks.Remove(task);
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
