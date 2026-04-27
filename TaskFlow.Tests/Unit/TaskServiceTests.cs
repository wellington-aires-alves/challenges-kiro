using TaskFlow.API;
using TaskFlow.Core;
using TaskFlow.Core.DTOs;
using TaskFlow.Core.Entities;
using TaskFlow.Core.Interfaces;
using TaskFlow.Core.Services;

namespace TaskFlow.Tests.Unit;

public class TaskServiceTests
{
    [Fact]
    public async Task Create_WithEmptyTitle_ReturnsValidationError()
    {
        // Arrange
        var taskRepository = new InMemoryTaskRepository();
        var taskService = new TaskService(taskRepository);
        var userId = Guid.NewGuid();

        var request = new CreateTaskRequest("", "Some description");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => taskService.CreateAsync(userId, request)
        );
        Assert.Equal("Título é obrigatório.", exception.Message);
    }

    [Fact]
    public async Task Create_WithTitleExceedingLimit_ReturnsValidationError()
    {
        // Arrange
        var taskRepository = new InMemoryTaskRepository();
        var taskService = new TaskService(taskRepository);
        var userId = Guid.NewGuid();

        var longTitle = new string('a', 201); // 201 characters
        var request = new CreateTaskRequest(longTitle, "Some description");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => taskService.CreateAsync(userId, request)
        );
        Assert.Equal("O título deve ter no máximo 200 caracteres.", exception.Message);
    }

    [Fact]
    public async Task Create_WithValidData_AssignsPendingStatusAndCreatedAt()
    {
        // Arrange
        var taskRepository = new InMemoryTaskRepository();
        var taskService = new TaskService(taskRepository);
        var userId = Guid.NewGuid();

        var beforeCreation = DateTime.UtcNow;
        var request = new CreateTaskRequest("Valid Task Title", "Task description");

        // Act
        var result = await taskService.CreateAsync(userId, request);
        var afterCreation = DateTime.UtcNow;

        // Assert
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("Valid Task Title", result.Title);
        Assert.Equal("Task description", result.Description);
        Assert.Equal("Pendente", result.Status);
        Assert.True(result.CreatedAt >= beforeCreation && result.CreatedAt <= afterCreation);
    }

    [Fact]
    public async Task Update_WithTaskBelongingToAnotherUser_ReturnsForbidden()
    {
        // Arrange
        var taskRepository = new InMemoryTaskRepository();
        var taskService = new TaskService(taskRepository);

        var ownerUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        // Create a task for the owner
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            UserId = ownerUserId,
            Title = "Owner's Task",
            Description = "Description",
            Status = TaskFlow.Core.Entities.TaskStatus.Pendente,
            CreatedAt = DateTime.UtcNow
        };
        await taskRepository.AddAsync(task);

        // Try to update with another user
        var updateRequest = new UpdateTaskRequest("Updated Title", "Updated Description", "Concluida");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ForbiddenException>(
            () => taskService.UpdateAsync(otherUserId, task.Id, updateRequest)
        );
        Assert.Equal("Acesso negado.", exception.Message);
    }

    [Fact]
    public async Task Update_WithNonExistentTask_ReturnsNotFound()
    {
        // Arrange
        var taskRepository = new InMemoryTaskRepository();
        var taskService = new TaskService(taskRepository);
        var userId = Guid.NewGuid();
        var nonExistentTaskId = Guid.NewGuid();

        var updateRequest = new UpdateTaskRequest("Updated Title", "Updated Description", "Concluida");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => taskService.UpdateAsync(userId, nonExistentTaskId, updateRequest)
        );
        Assert.Equal("Tarefa não encontrada.", exception.Message);
    }

    [Fact]
    public async Task Update_WithInvalidStatus_ReturnsValidationError()
    {
        // Arrange
        var taskRepository = new InMemoryTaskRepository();
        var taskService = new TaskService(taskRepository);
        var userId = Guid.NewGuid();

        // Create a task
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = "Task Title",
            Description = "Description",
            Status = TaskFlow.Core.Entities.TaskStatus.Pendente,
            CreatedAt = DateTime.UtcNow
        };
        await taskRepository.AddAsync(task);

        // Try to update with invalid status
        var updateRequest = new UpdateTaskRequest("Updated Title", "Updated Description", "InvalidStatus");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => taskService.UpdateAsync(userId, task.Id, updateRequest)
        );
        Assert.Equal("Status inválido. Valores aceitos: Pendente, Concluida.", exception.Message);
    }

    [Fact]
    public async Task Delete_WithTaskBelongingToAnotherUser_ReturnsForbidden()
    {
        // Arrange
        var taskRepository = new InMemoryTaskRepository();
        var taskService = new TaskService(taskRepository);

        var ownerUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        // Create a task for the owner
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            UserId = ownerUserId,
            Title = "Owner's Task",
            Description = "Description",
            Status = TaskFlow.Core.Entities.TaskStatus.Pendente,
            CreatedAt = DateTime.UtcNow
        };
        await taskRepository.AddAsync(task);

        // Try to delete with another user
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ForbiddenException>(
            () => taskService.DeleteAsync(otherUserId, task.Id)
        );
        Assert.Equal("Acesso negado.", exception.Message);
    }

    [Fact]
    public async Task Delete_WithNonExistentTask_ReturnsNotFound()
    {
        // Arrange
        var taskRepository = new InMemoryTaskRepository();
        var taskService = new TaskService(taskRepository);
        var userId = Guid.NewGuid();
        var nonExistentTaskId = Guid.NewGuid();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => taskService.DeleteAsync(userId, nonExistentTaskId)
        );
        Assert.Equal("Tarefa não encontrada.", exception.Message);
    }

    [Fact]
    public async Task GetAll_ReturnsOnlyTasksOfAuthenticatedUser()
    {
        // Arrange
        var taskRepository = new InMemoryTaskRepository();
        var taskService = new TaskService(taskRepository);

        var user1Id = Guid.NewGuid();
        var user2Id = Guid.NewGuid();

        // Create tasks for user 1
        var task1User1 = new TaskItem
        {
            Id = Guid.NewGuid(),
            UserId = user1Id,
            Title = "User 1 Task 1",
            Description = "Description",
            Status = TaskFlow.Core.Entities.TaskStatus.Pendente,
            CreatedAt = DateTime.UtcNow
        };
        await taskRepository.AddAsync(task1User1);

        var task2User1 = new TaskItem
        {
            Id = Guid.NewGuid(),
            UserId = user1Id,
            Title = "User 1 Task 2",
            Description = "Description",
            Status = TaskFlow.Core.Entities.TaskStatus.Concluida,
            CreatedAt = DateTime.UtcNow
        };
        await taskRepository.AddAsync(task2User1);

        // Create tasks for user 2
        var task1User2 = new TaskItem
        {
            Id = Guid.NewGuid(),
            UserId = user2Id,
            Title = "User 2 Task 1",
            Description = "Description",
            Status = TaskFlow.Core.Entities.TaskStatus.Pendente,
            CreatedAt = DateTime.UtcNow
        };
        await taskRepository.AddAsync(task1User2);

        // Act
        var user1Tasks = await taskService.GetAllAsync(user1Id);
        var user2Tasks = await taskService.GetAllAsync(user2Id);

        // Assert
        var user1TasksList = user1Tasks.ToList();
        var user2TasksList = user2Tasks.ToList();

        Assert.Equal(2, user1TasksList.Count);
        Assert.All(user1TasksList, task => Assert.Contains("User 1", task.Title));

        Assert.Single(user2TasksList);
        Assert.Contains("User 2", user2TasksList[0].Title);
    }

    [Fact]
    public async Task GetAll_ReturnsTasksOrderedByCreatedAtDescending()
    {
        // Arrange
        var taskRepository = new InMemoryTaskRepository();
        var taskService = new TaskService(taskRepository);
        var userId = Guid.NewGuid();

        // Create tasks with different creation times
        var task1 = new TaskItem
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = "First Task",
            Description = "Description",
            Status = TaskFlow.Core.Entities.TaskStatus.Pendente,
            CreatedAt = DateTime.UtcNow.AddMinutes(-10)
        };
        await taskRepository.AddAsync(task1);

        var task2 = new TaskItem
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = "Second Task",
            Description = "Description",
            Status = TaskFlow.Core.Entities.TaskStatus.Pendente,
            CreatedAt = DateTime.UtcNow.AddMinutes(-5)
        };
        await taskRepository.AddAsync(task2);

        var task3 = new TaskItem
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = "Third Task",
            Description = "Description",
            Status = TaskFlow.Core.Entities.TaskStatus.Pendente,
            CreatedAt = DateTime.UtcNow
        };
        await taskRepository.AddAsync(task3);

        // Act
        var tasks = await taskService.GetAllAsync(userId);
        var tasksList = tasks.ToList();

        // Assert
        Assert.Equal(3, tasksList.Count);
        Assert.Equal("Third Task", tasksList[0].Title); // Most recent
        Assert.Equal("Second Task", tasksList[1].Title);
        Assert.Equal("First Task", tasksList[2].Title); // Oldest
        Assert.True(tasksList[0].CreatedAt >= tasksList[1].CreatedAt);
        Assert.True(tasksList[1].CreatedAt >= tasksList[2].CreatedAt);
    }
}
