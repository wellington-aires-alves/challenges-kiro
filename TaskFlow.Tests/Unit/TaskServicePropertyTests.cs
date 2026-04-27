using TaskFlow.API;
using TaskFlow.Core;
using TaskFlow.Core.DTOs;
using TaskFlow.Core.Entities;
using TaskFlow.Core.Interfaces;
using TaskFlow.Core.Services;

namespace TaskFlow.Tests.Unit;

/// <summary>
/// Property-based tests for TaskService
/// </summary>
public class TaskServicePropertyTests
{
    // Feature: taskflow, Property 5: Isolamento de tarefas por usuário
    /// <summary>
    /// **Validates: Requirements 3.4, 5.1**
    /// 
    /// Property: For any pair of distinct users, the task listing of one user
    /// should never contain tasks belonging to the other user.
    /// 
    /// This test runs 100 iterations with randomly generated users and tasks to verify
    /// that task isolation is always maintained regardless of the specific values.
    /// </summary>
    [Fact]
    public async Task GetAll_NeverReturnsTasksFromAnotherUser()
    {
        var random = new Random(44); // Fixed seed for reproducibility
        var failedIterations = new List<string>();

        // Run 100 iterations as specified in the design document
        for (int i = 0; i < 100; i++)
        {
            try
            {
                // Arrange
                var taskRepository = new InMemoryTaskRepository();
                var taskService = new TaskService(taskRepository);

                // Generate two distinct user IDs
                var user1Id = Guid.NewGuid();
                var user2Id = Guid.NewGuid();

                // Ensure users are distinct
                while (user1Id == user2Id)
                {
                    user2Id = Guid.NewGuid();
                }

                // Generate random number of tasks for each user (1-10 tasks each)
                var user1TaskCount = random.Next(1, 11);
                var user2TaskCount = random.Next(1, 11);

                var user1TaskIds = new List<Guid>();
                var user2TaskIds = new List<Guid>();

                // Create tasks for user 1
                for (int j = 0; j < user1TaskCount; j++)
                {
                    var task = new TaskItem
                    {
                        Id = Guid.NewGuid(),
                        UserId = user1Id,
                        Title = $"User1_Task_{i}_{j}_{random.Next(1000000)}",
                        Description = $"Description for user 1 task {j}",
                        Status = random.Next(2) == 0 ? TaskFlow.Core.Entities.TaskStatus.Pendente : TaskFlow.Core.Entities.TaskStatus.Concluida,
                        CreatedAt = DateTime.UtcNow.AddMinutes(-random.Next(1000))
                    };
                    await taskRepository.AddAsync(task);
                    user1TaskIds.Add(task.Id);
                }

                // Create tasks for user 2
                for (int j = 0; j < user2TaskCount; j++)
                {
                    var task = new TaskItem
                    {
                        Id = Guid.NewGuid(),
                        UserId = user2Id,
                        Title = $"User2_Task_{i}_{j}_{random.Next(1000000)}",
                        Description = $"Description for user 2 task {j}",
                        Status = random.Next(2) == 0 ? TaskFlow.Core.Entities.TaskStatus.Pendente : TaskFlow.Core.Entities.TaskStatus.Concluida,
                        CreatedAt = DateTime.UtcNow.AddMinutes(-random.Next(1000))
                    };
                    await taskRepository.AddAsync(task);
                    user2TaskIds.Add(task.Id);
                }

                // Act - Get tasks for both users
                var user1Tasks = await taskService.GetAllAsync(user1Id);
                var user2Tasks = await taskService.GetAllAsync(user2Id);

                var user1TasksList = user1Tasks.ToList();
                var user2TasksList = user2Tasks.ToList();

                // Assert - User 1 should only see their own tasks
                Assert.Equal(user1TaskCount, user1TasksList.Count);
                foreach (var task in user1TasksList)
                {
                    Assert.Contains(task.Id, user1TaskIds);
                    Assert.DoesNotContain(task.Id, user2TaskIds);
                    Assert.True(task.Title.StartsWith("User1_Task_"), 
                        $"Iteration {i}: User 1 task should have User1 prefix, but got: {task.Title}");
                }

                // Assert - User 2 should only see their own tasks
                Assert.Equal(user2TaskCount, user2TasksList.Count);
                foreach (var task in user2TasksList)
                {
                    Assert.Contains(task.Id, user2TaskIds);
                    Assert.DoesNotContain(task.Id, user1TaskIds);
                    Assert.True(task.Title.StartsWith("User2_Task_"), 
                        $"Iteration {i}: User 2 task should have User2 prefix, but got: {task.Title}");
                }

                // Assert - No overlap between user task lists
                var user1TaskIdSet = user1TasksList.Select(t => t.Id).ToHashSet();
                var user2TaskIdSet = user2TasksList.Select(t => t.Id).ToHashSet();
                
                Assert.Empty(user1TaskIdSet.Intersect(user2TaskIdSet));
            }
            catch (Exception ex)
            {
                failedIterations.Add($"Iteration {i}: {ex.Message}");
            }
        }

        // If any iterations failed, report them
        if (failedIterations.Any())
        {
            Assert.Fail($"Property test failed in {failedIterations.Count} out of 100 iterations:\n" + 
                       string.Join("\n", failedIterations));
        }
    }

    // Feature: taskflow, Property 7: Status inicial de nova tarefa
    /// <summary>
    /// **Validates: Requirements 4.4, 4.5**
    /// 
    /// Property: For any task created with a valid title, the status should be "Pendente"
    /// and CreatedAt should be populated at the moment of creation.
    /// 
    /// This test runs 100 iterations with randomly generated valid titles to verify
    /// that initial status and timestamp are always correctly assigned.
    /// </summary>
    [Fact]
    public async Task Create_InitialStatusAlwaysPending()
    {
        var random = new Random(46); // Fixed seed for reproducibility
        var failedIterations = new List<string>();

        // Run 100 iterations as specified in the design document
        for (int i = 0; i < 100; i++)
        {
            try
            {
                // Arrange
                var taskRepository = new InMemoryTaskRepository();
                var taskService = new TaskService(taskRepository);
                var userId = Guid.NewGuid();

                // Generate random valid title (1-200 characters, not whitespace-only)
                var titleLength = random.Next(1, 201);
                var title = GenerateRandomValidTitle(random, titleLength);

                // Generate random description (50% null, 50% with content)
                string? description = random.Next(2) == 0 
                    ? null 
                    : $"Description {i}_{random.Next(1000000)}";

                // Capture time before creation
                var beforeCreation = DateTime.UtcNow;
                
                // Small delay to ensure time precision
                await Task.Delay(1);

                // Act
                var createRequest = new CreateTaskRequest(title, description);
                var result = await taskService.CreateAsync(userId, createRequest);

                // Small delay to ensure time precision
                await Task.Delay(1);
                
                // Capture time after creation
                var afterCreation = DateTime.UtcNow;

                // Assert - Status should always be "Pendente"
                Assert.Equal("Pendente", result.Status);

                // Assert - CreatedAt should be populated and within the time window
                Assert.NotEqual(default(DateTime), result.CreatedAt);
                Assert.True(result.CreatedAt >= beforeCreation, 
                    $"Iteration {i}: CreatedAt ({result.CreatedAt:O}) should be >= beforeCreation ({beforeCreation:O})");
                Assert.True(result.CreatedAt <= afterCreation, 
                    $"Iteration {i}: CreatedAt ({result.CreatedAt:O}) should be <= afterCreation ({afterCreation:O})");

                // Assert - Verify the task was actually stored with correct values
                var storedTask = await taskRepository.GetByIdAsync(result.Id);
                Assert.NotNull(storedTask);
                Assert.Equal(TaskFlow.Core.Entities.TaskStatus.Pendente, storedTask.Status);
                Assert.Equal(result.CreatedAt, storedTask.CreatedAt);

                // Assert - Other fields should match the request
                Assert.Equal(title, result.Title);
                Assert.Equal(description, result.Description);
                Assert.NotEqual(Guid.Empty, result.Id);
            }
            catch (Exception ex)
            {
                failedIterations.Add($"Iteration {i}: {ex.Message}");
            }
        }

        // If any iterations failed, report them
        if (failedIterations.Any())
        {
            Assert.Fail($"Property test failed in {failedIterations.Count} out of 100 iterations:\n" + 
                       string.Join("\n", failedIterations));
        }
    }

    // Feature: taskflow, Property 8: Ordenação da listagem por data de criação
    /// <summary>
    /// **Validates: Requirement 5.3**
    /// 
    /// Property: For any set of tasks belonging to a user, the listing returned
    /// should always be ordered by CreatedAt in descending order (most recent first).
    /// 
    /// This test runs 100 iterations with randomly generated tasks with different
    /// CreatedAt timestamps to verify that ordering is always maintained.
    /// </summary>
    [Fact]
    public async Task GetAll_AlwaysOrderedByCreatedAtDescending()
    {
        var random = new Random(47); // Fixed seed for reproducibility
        var failedIterations = new List<string>();

        // Run 100 iterations as specified in the design document
        for (int i = 0; i < 100; i++)
        {
            try
            {
                // Arrange
                var taskRepository = new InMemoryTaskRepository();
                var taskService = new TaskService(taskRepository);
                var userId = Guid.NewGuid();

                // Generate random number of tasks (2-20 tasks to ensure meaningful ordering test)
                var taskCount = random.Next(2, 21);
                var createdTasks = new List<TaskItem>();

                // Create a base timestamp
                var baseTime = DateTime.UtcNow.AddDays(-random.Next(1, 365));

                // Create tasks with different CreatedAt timestamps
                for (int j = 0; j < taskCount; j++)
                {
                    // Generate random offset from base time (can be negative or positive)
                    // This ensures we have a good mix of timestamps
                    var minutesOffset = random.Next(-10000, 10000);
                    
                    var task = new TaskItem
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        Title = $"Task_{i}_{j}_{random.Next(1000000)}",
                        Description = $"Description for task {j}",
                        Status = random.Next(2) == 0 
                            ? TaskFlow.Core.Entities.TaskStatus.Pendente 
                            : TaskFlow.Core.Entities.TaskStatus.Concluida,
                        CreatedAt = baseTime.AddMinutes(minutesOffset)
                    };
                    
                    await taskRepository.AddAsync(task);
                    createdTasks.Add(task);
                }

                // Act - Get all tasks for the user
                var result = await taskService.GetAllAsync(userId);
                var resultList = result.ToList();

                // Assert - Should return all tasks
                Assert.Equal(taskCount, resultList.Count);

                // Assert - Tasks should be ordered by CreatedAt descending
                for (int j = 0; j < resultList.Count - 1; j++)
                {
                    var currentTask = resultList[j];
                    var nextTask = resultList[j + 1];

                    Assert.True(
                        currentTask.CreatedAt >= nextTask.CreatedAt,
                        $"Iteration {i}: Task at index {j} (CreatedAt: {currentTask.CreatedAt:O}) " +
                        $"should have CreatedAt >= task at index {j + 1} (CreatedAt: {nextTask.CreatedAt:O}). " +
                        $"Tasks are not properly ordered by CreatedAt descending."
                    );
                }

                // Additional verification: Compare with expected sorted order
                var expectedOrder = createdTasks
                    .OrderByDescending(t => t.CreatedAt)
                    .Select(t => t.Id)
                    .ToList();

                var actualOrder = resultList.Select(t => t.Id).ToList();

                Assert.Equal(expectedOrder, actualOrder);

                // Verify that the earliest and latest tasks are in correct positions
                var earliestTask = createdTasks.MinBy(t => t.CreatedAt);
                var latestTask = createdTasks.MaxBy(t => t.CreatedAt);

                Assert.Equal(latestTask!.Id, resultList.First().Id);
                Assert.Equal(earliestTask!.Id, resultList.Last().Id);
            }
            catch (Exception ex)
            {
                failedIterations.Add($"Iteration {i}: {ex.Message}");
            }
        }

        // If any iterations failed, report them
        if (failedIterations.Any())
        {
            Assert.Fail($"Property test failed in {failedIterations.Count} out of 100 iterations:\n" + 
                       string.Join("\n", failedIterations));
        }
    }

    // Feature: taskflow, Property 9: Proteção de tarefa de outro usuário (edição e exclusão)
    /// <summary>
    /// **Validates: Requirements 6.3, 7.3**
    /// 
    /// Property: For any pair of distinct users and any task belonging to the first user,
    /// an attempt by the second user to edit or delete that task should be rejected with
    /// ForbiddenException and the task should remain unchanged.
    /// 
    /// This test runs 100 iterations with randomly generated users and tasks to verify
    /// that task protection is always enforced regardless of the specific values.
    /// </summary>
    [Fact]
    public async Task UpdateDelete_TaskOfAnotherUser_AlwaysReturns403()
    {
        var random = new Random(48); // Fixed seed for reproducibility
        var failedIterations = new List<string>();

        // Run 100 iterations as specified in the design document
        for (int i = 0; i < 100; i++)
        {
            try
            {
                // Arrange
                var taskRepository = new InMemoryTaskRepository();
                var taskService = new TaskService(taskRepository);

                // Generate two distinct user IDs
                var ownerUserId = Guid.NewGuid();
                var otherUserId = Guid.NewGuid();

                // Ensure users are distinct
                while (ownerUserId == otherUserId)
                {
                    otherUserId = Guid.NewGuid();
                }

                // Create a task owned by the first user
                var taskTitle = $"OwnerTask_{i}_{random.Next(1000000)}";
                var taskDescription = $"Description for iteration {i}";
                var taskStatus = random.Next(2) == 0 
                    ? TaskFlow.Core.Entities.TaskStatus.Pendente 
                    : TaskFlow.Core.Entities.TaskStatus.Concluida;

                var ownerTask = new TaskItem
                {
                    Id = Guid.NewGuid(),
                    UserId = ownerUserId,
                    Title = taskTitle,
                    Description = taskDescription,
                    Status = taskStatus,
                    CreatedAt = DateTime.UtcNow.AddMinutes(-random.Next(1000))
                };

                await taskRepository.AddAsync(ownerTask);

                // Store original task state for verification
                var originalTitle = ownerTask.Title;
                var originalDescription = ownerTask.Description;
                var originalStatus = ownerTask.Status;
                var originalCreatedAt = ownerTask.CreatedAt;

                // Act & Assert - Test UPDATE operation by other user
                var updateRequest = new UpdateTaskRequest(
                    $"Modified_{random.Next(1000000)}",
                    "Modified description",
                    "Concluida"
                );

                var updateException = await Assert.ThrowsAsync<ForbiddenException>(
                    async () => await taskService.UpdateAsync(otherUserId, ownerTask.Id, updateRequest)
                );

                // Verify the task remained unchanged after failed update attempt
                var taskAfterUpdate = await taskRepository.GetByIdAsync(ownerTask.Id);
                Assert.NotNull(taskAfterUpdate);
                Assert.Equal(originalTitle, taskAfterUpdate.Title);
                Assert.Equal(originalDescription, taskAfterUpdate.Description);
                Assert.Equal(originalStatus, taskAfterUpdate.Status);
                Assert.Equal(originalCreatedAt, taskAfterUpdate.CreatedAt);
                Assert.Equal(ownerUserId, taskAfterUpdate.UserId);

                // Verify error message is appropriate
                Assert.Contains("acesso", updateException.Message.ToLower());

                // Act & Assert - Test DELETE operation by other user
                var deleteException = await Assert.ThrowsAsync<ForbiddenException>(
                    async () => await taskService.DeleteAsync(otherUserId, ownerTask.Id)
                );

                // Verify the task still exists after failed delete attempt
                var taskAfterDelete = await taskRepository.GetByIdAsync(ownerTask.Id);
                Assert.NotNull(taskAfterDelete);
                Assert.Equal(originalTitle, taskAfterDelete.Title);
                Assert.Equal(originalDescription, taskAfterDelete.Description);
                Assert.Equal(originalStatus, taskAfterDelete.Status);
                Assert.Equal(originalCreatedAt, taskAfterDelete.CreatedAt);
                Assert.Equal(ownerUserId, taskAfterDelete.UserId);

                // Verify error message is appropriate
                Assert.Contains("acesso", deleteException.Message.ToLower());

                // Additional verification: Ensure the owner can still access and modify their task
                var ownerTasks = await taskService.GetAllAsync(ownerUserId);
                Assert.Contains(ownerTasks, t => t.Id == ownerTask.Id);

                // Verify the owner can successfully update their own task
                var ownerUpdateRequest = new UpdateTaskRequest(
                    "Owner Updated Title",
                    "Owner updated description",
                    "Concluida"
                );
                var ownerUpdateResult = await taskService.UpdateAsync(ownerUserId, ownerTask.Id, ownerUpdateRequest);
                Assert.NotNull(ownerUpdateResult);
                Assert.Equal("Owner Updated Title", ownerUpdateResult.Title);

                // Verify the owner can successfully delete their own task
                await taskService.DeleteAsync(ownerUserId, ownerTask.Id);
                var taskAfterOwnerDelete = await taskRepository.GetByIdAsync(ownerTask.Id);
                Assert.Null(taskAfterOwnerDelete);
            }
            catch (Exception ex)
            {
                failedIterations.Add($"Iteration {i}: {ex.Message}");
            }
        }

        // If any iterations failed, report them
        if (failedIterations.Any())
        {
            Assert.Fail($"Property test failed in {failedIterations.Count} out of 100 iterations:\n" + 
                       string.Join("\n", failedIterations));
        }
    }

    // Feature: taskflow, Property 10: Round-trip de dados da tarefa
    /// <summary>
    /// **Validates: Requirement 5.4**
    /// 
    /// Property: For any task created with valid title and description, the data returned
    /// by the listing should contain exactly the same Id, Title, Description, Status, and CreatedAt
    /// as the created task.
    /// 
    /// This test runs 100 iterations with randomly generated valid task data to verify
    /// that data integrity is always maintained through the create-and-retrieve cycle.
    /// </summary>
    [Fact]
    public async Task Create_ReturnedDataMatchesCreatedData()
    {
        var random = new Random(49); // Fixed seed for reproducibility
        var failedIterations = new List<string>();

        // Run 100 iterations as specified in the design document
        for (int i = 0; i < 100; i++)
        {
            try
            {
                // Arrange
                var taskRepository = new InMemoryTaskRepository();
                var taskService = new TaskService(taskRepository);
                var userId = Guid.NewGuid();

                // Generate random valid title (1-200 characters, not whitespace-only)
                var titleLength = random.Next(1, 201);
                var title = GenerateRandomValidTitle(random, titleLength);

                // Generate random description (50% null, 50% with content)
                string? description = random.Next(2) == 0 
                    ? null 
                    : $"Description for iteration {i} with random data {random.Next(1000000)}";

                // Act - Create the task
                var createRequest = new CreateTaskRequest(title, description);
                var createdTask = await taskService.CreateAsync(userId, createRequest);

                // Act - Retrieve the task via GetAllAsync
                var allTasks = await taskService.GetAllAsync(userId);
                var retrievedTask = allTasks.FirstOrDefault(t => t.Id == createdTask.Id);

                // Assert - Task should be found in the listing
                Assert.NotNull(retrievedTask);

                // Assert - Id should match exactly
                Assert.Equal(createdTask.Id, retrievedTask.Id);

                // Assert - Title should match exactly
                Assert.Equal(createdTask.Title, retrievedTask.Title);
                Assert.Equal(title, retrievedTask.Title);

                // Assert - Description should match exactly (including null)
                Assert.Equal(createdTask.Description, retrievedTask.Description);
                Assert.Equal(description, retrievedTask.Description);

                // Assert - Status should match exactly
                Assert.Equal(createdTask.Status, retrievedTask.Status);
                Assert.Equal("Pendente", retrievedTask.Status);

                // Assert - CreatedAt should match exactly
                Assert.Equal(createdTask.CreatedAt, retrievedTask.CreatedAt);

                // Additional verification: Verify all fields are non-default
                Assert.NotEqual(Guid.Empty, retrievedTask.Id);
                Assert.NotEmpty(retrievedTask.Title);
                Assert.NotEqual(default(DateTime), retrievedTask.CreatedAt);

                // Additional verification: Verify the task in repository matches as well
                var taskFromRepo = await taskRepository.GetByIdAsync(createdTask.Id);
                Assert.NotNull(taskFromRepo);
                Assert.Equal(createdTask.Id, taskFromRepo.Id);
                Assert.Equal(title, taskFromRepo.Title);
                Assert.Equal(description, taskFromRepo.Description);
                Assert.Equal(TaskFlow.Core.Entities.TaskStatus.Pendente, taskFromRepo.Status);
                Assert.Equal(createdTask.CreatedAt, taskFromRepo.CreatedAt);
                Assert.Equal(userId, taskFromRepo.UserId);
            }
            catch (Exception ex)
            {
                failedIterations.Add($"Iteration {i}: {ex.Message}");
            }
        }

        // If any iterations failed, report them
        if (failedIterations.Any())
        {
            Assert.Fail($"Property test failed in {failedIterations.Count} out of 100 iterations:\n" + 
                       string.Join("\n", failedIterations));
        }
    }

    /// <summary>
    /// Helper method to generate random valid titles (non-whitespace-only, 1-200 chars)
    /// </summary>
    private static string GenerateRandomValidTitle(Random random, int length)
    {
        // Ensure at least one non-whitespace character
        var chars = new char[length];
        
        // First character is always non-whitespace
        chars[0] = (char)random.Next('A', 'Z' + 1);
        
        // Rest can be alphanumeric or spaces (but not all spaces)
        for (int i = 1; i < length; i++)
        {
            // 80% alphanumeric, 20% space
            if (random.Next(100) < 80)
            {
                // Alphanumeric
                var charType = random.Next(3);
                chars[i] = charType switch
                {
                    0 => (char)random.Next('A', 'Z' + 1), // Uppercase
                    1 => (char)random.Next('a', 'z' + 1), // Lowercase
                    _ => (char)random.Next('0', '9' + 1)  // Digit
                };
            }
            else
            {
                chars[i] = ' ';
            }
        }
        
        return new string(chars);
    }

    // Feature: taskflow, Property 6: Validação de título na criação e edição
    /// <summary>
    /// **Validates: Requirements 4.2, 4.3, 6.4, 6.5**
    /// 
    /// Property: For any title composed entirely of whitespace or with more than 200 characters,
    /// the create or update operation should be rejected and the repository should remain unchanged.
    /// 
    /// This test runs 100 iterations with randomly generated invalid titles to verify
    /// that validation is always enforced regardless of the specific invalid values.
    /// </summary>
    [Fact]
    public async Task Create_InvalidTitle_AlwaysRejects()
    {
        var random = new Random(45); // Fixed seed for reproducibility
        var failedIterations = new List<string>();

        // Run 100 iterations as specified in the design document
        for (int i = 0; i < 100; i++)
        {
            try
            {
                // Arrange
                var taskRepository = new InMemoryTaskRepository();
                var taskService = new TaskService(taskRepository);
                var userId = Guid.NewGuid();

                // Generate invalid title (50% whitespace-only, 50% exceeding 200 chars)
                string invalidTitle;
                bool isWhitespaceOnly = random.Next(2) == 0;

                if (isWhitespaceOnly)
                {
                    // Generate whitespace-only title (spaces, tabs, newlines)
                    var whitespaceChars = new[] { ' ', '\t', '\n', '\r' };
                    var length = random.Next(1, 50);
                    invalidTitle = new string(Enumerable.Range(0, length)
                        .Select(_ => whitespaceChars[random.Next(whitespaceChars.Length)])
                        .ToArray());
                }
                else
                {
                    // Generate title exceeding 200 characters
                    var length = random.Next(201, 500);
                    invalidTitle = new string('A', length);
                }

                // Get initial repository state
                var initialTasks = await taskRepository.GetByUserIdAsync(userId);
                var initialCount = initialTasks.Count();

                // Act & Assert - Create operation should reject invalid title
                var createRequest = new CreateTaskRequest(invalidTitle, "Valid description");
                
                var createException = await Assert.ThrowsAsync<ValidationException>(
                    async () => await taskService.CreateAsync(userId, createRequest)
                );

                // Verify repository remained unchanged
                var tasksAfterCreate = await taskRepository.GetByUserIdAsync(userId);
                Assert.Equal(initialCount, tasksAfterCreate.Count());

                // Now test Update operation with invalid title
                // First, create a valid task
                var validTask = new TaskItem
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Title = "Valid Title",
                    Description = "Valid Description",
                    Status = TaskFlow.Core.Entities.TaskStatus.Pendente,
                    CreatedAt = DateTime.UtcNow
                };
                await taskRepository.AddAsync(validTask);

                // Store original task state
                var originalTitle = validTask.Title;
                var originalDescription = validTask.Description;
                var originalStatus = validTask.Status;

                // Act & Assert - Update operation should reject invalid title
                var updateRequest = new UpdateTaskRequest(invalidTitle, "Updated description", "Pendente");
                
                var updateException = await Assert.ThrowsAsync<ValidationException>(
                    async () => await taskService.UpdateAsync(userId, validTask.Id, updateRequest)
                );

                // Verify task remained unchanged
                var taskAfterUpdate = await taskRepository.GetByIdAsync(validTask.Id);
                Assert.NotNull(taskAfterUpdate);
                Assert.Equal(originalTitle, taskAfterUpdate.Title);
                Assert.Equal(originalDescription, taskAfterUpdate.Description);
                Assert.Equal(originalStatus, taskAfterUpdate.Status);

                // Verify error messages are appropriate
                Assert.Contains("título", createException.Message.ToLower());
                Assert.Contains("título", updateException.Message.ToLower());
            }
            catch (Exception ex)
            {
                failedIterations.Add($"Iteration {i}: {ex.Message}");
            }
        }

        // If any iterations failed, report them
        if (failedIterations.Any())
        {
            Assert.Fail($"Property test failed in {failedIterations.Count} out of 100 iterations:\n" + 
                       string.Join("\n", failedIterations));
        }
    }
}
