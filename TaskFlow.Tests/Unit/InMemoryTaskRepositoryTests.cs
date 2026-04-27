using TaskFlow.API;
using TaskFlow.Core.Entities;
using TaskStatus = TaskFlow.Core.Entities.TaskStatus;

namespace TaskFlow.Tests.Unit;

public class InMemoryTaskRepositoryTests
{
    [Fact]
    public async Task Add_AndGetById_ReturnsAddedItem()
    {
        // Arrange
        var repository = new InMemoryTaskRepository();
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Title = "Test Task",
            Description = "Test Description",
            Status = TaskStatus.Pendente,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        await repository.AddAsync(task);
        var result = await repository.GetByIdAsync(task.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(task.Id, result.Id);
        Assert.Equal(task.UserId, result.UserId);
        Assert.Equal(task.Title, result.Title);
        Assert.Equal(task.Description, result.Description);
        Assert.Equal(task.Status, result.Status);
    }

    [Fact]
    public async Task Delete_RemovesItemFromRepository()
    {
        // Arrange
        var repository = new InMemoryTaskRepository();
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Title = "Test Task",
            Description = "Test Description",
            Status = TaskStatus.Pendente,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        await repository.AddAsync(task);
        var addedTask = await repository.GetByIdAsync(task.Id);
        Assert.NotNull(addedTask); // Verify task was added

        await repository.DeleteAsync(task.Id);
        var deletedTask = await repository.GetByIdAsync(task.Id);

        // Assert
        Assert.Null(deletedTask);
    }
}
