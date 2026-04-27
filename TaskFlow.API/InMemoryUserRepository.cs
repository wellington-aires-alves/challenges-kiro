using TaskFlow.Core.Entities;
using TaskFlow.Core.Interfaces;

namespace TaskFlow.API;

public class InMemoryUserRepository : IUserRepository
{
    private readonly List<User> _users = new();
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public async Task<User?> GetByEmailAsync(string email)
    {
        await _semaphore.WaitAsync();
        try
        {
            return _users.FirstOrDefault(u =>
                u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        await _semaphore.WaitAsync();
        try
        {
            return _users.FirstOrDefault(u =>
                u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        await _semaphore.WaitAsync();
        try
        {
            return _users.FirstOrDefault(u => u.Id == id);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task AddAsync(User user)
    {
        await _semaphore.WaitAsync();
        try
        {
            _users.Add(user);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        await _semaphore.WaitAsync();
        try
        {
            return _users.Any(u =>
                u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<bool> ExistsByUsernameAsync(string username)
    {
        await _semaphore.WaitAsync();
        try
        {
            return _users.Any(u =>
                u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
