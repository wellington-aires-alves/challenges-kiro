using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.IdentityModel.Tokens;
using TaskFlow.Core.DTOs;

namespace TaskFlow.Tests.Integration;

/// <summary>
/// Integration tests for TaskFlow API endpoints using WebApplicationFactory.
/// Tests the complete HTTP pipeline including authentication, authorization, and CRUD operations.
/// **Validates: Requirements 3.1, 3.2, 3.3, 4.1, 5.1, 6.1, 7.1**
/// </summary>
public class ApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    /// <summary>
    /// Tests the complete authentication flow: register a user, login, and verify token is returned.
    /// **Validates: Requirements 1.1, 1.2, 2.1, 2.2**
    /// </summary>
    [Fact]
    public async Task AuthEndpoints_Register_Login_CompleteFlow()
    {
        // Arrange
        var username = $"testuser_{Guid.NewGuid():N}";
        var email = $"{username}@test.com";
        var password = "password123";

        var registerRequest = new RegisterRequest(username, email, password);

        // Act - Register
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Assert - Register
        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);
        var registerResult = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(registerResult);
        Assert.NotNull(registerResult.Token);
        Assert.Equal(username, registerResult.Username);

        // Act - Login
        var loginRequest = new LoginRequest(email, password);
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert - Login
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(loginResult);
        Assert.NotNull(loginResult.Token);
        Assert.Equal(username, loginResult.Username);

        // Verify token is valid JWT
        var handler = new JwtSecurityTokenHandler();
        Assert.True(handler.CanReadToken(loginResult.Token));
        var token = handler.ReadJwtToken(loginResult.Token);
        Assert.Equal("TaskFlow", token.Issuer);
        Assert.Contains(token.Audiences, a => a == "TaskFlow");
    }

    /// <summary>
    /// Tests that accessing task endpoints without a token returns HTTP 401.
    /// **Validates: Requirements 3.1**
    /// </summary>
    [Fact]
    public async Task TasksEndpoints_WithoutToken_Returns401()
    {
        // Act & Assert - GET /api/tasks
        var getResponse = await _client.GetAsync("/api/tasks");
        Assert.Equal(HttpStatusCode.Unauthorized, getResponse.StatusCode);

        // Act & Assert - POST /api/tasks
        var createRequest = new CreateTaskRequest("Test Task", "Description");
        var postResponse = await _client.PostAsJsonAsync("/api/tasks", createRequest);
        Assert.Equal(HttpStatusCode.Unauthorized, postResponse.StatusCode);

        // Act & Assert - PUT /api/tasks/{id}
        var updateRequest = new UpdateTaskRequest("Updated", "Desc", "Pendente");
        var putResponse = await _client.PutAsJsonAsync($"/api/tasks/{Guid.NewGuid()}", updateRequest);
        Assert.Equal(HttpStatusCode.Unauthorized, putResponse.StatusCode);

        // Act & Assert - DELETE /api/tasks/{id}
        var deleteResponse = await _client.DeleteAsync($"/api/tasks/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.Unauthorized, deleteResponse.StatusCode);
    }

    /// <summary>
    /// Tests that accessing task endpoints with an expired token returns HTTP 401.
    /// **Validates: Requirements 3.3**
    /// </summary>
    [Fact]
    public async Task TasksEndpoints_WithExpiredToken_Returns401()
    {
        // Arrange - Create an expired token
        var expiredToken = GenerateExpiredJwtToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", expiredToken);

        // Act & Assert - GET /api/tasks
        var getResponse = await _client.GetAsync("/api/tasks");
        Assert.Equal(HttpStatusCode.Unauthorized, getResponse.StatusCode);

        // Act & Assert - POST /api/tasks
        var createRequest = new CreateTaskRequest("Test Task", "Description");
        var postResponse = await _client.PostAsJsonAsync("/api/tasks", createRequest);
        Assert.Equal(HttpStatusCode.Unauthorized, postResponse.StatusCode);
    }

    /// <summary>
    /// Tests the complete CRUD flow: create, list, update, and delete a task with a valid token.
    /// **Validates: Requirements 4.1, 5.1, 6.1, 7.1**
    /// </summary>
    [Fact]
    public async Task TasksEndpoints_CRUD_CompleteFlow()
    {
        // Arrange - Register and login to get a valid token
        var (token, userId) = await RegisterAndLoginAsync();
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act - Create a task
        var createRequest = new CreateTaskRequest("Integration Test Task", "This is a test description");
        var createResponse = await client.PostAsJsonAsync("/api/tasks", createRequest);

        // Assert - Create
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var createdTask = await createResponse.Content.ReadFromJsonAsync<TaskItemDto>();
        Assert.NotNull(createdTask);
        Assert.Equal("Integration Test Task", createdTask.Title);
        Assert.Equal("This is a test description", createdTask.Description);
        Assert.Equal("Pendente", createdTask.Status);
        Assert.NotEqual(Guid.Empty, createdTask.Id);

        // Act - List tasks
        var listResponse = await client.GetAsync("/api/tasks");

        // Assert - List
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        var tasks = await listResponse.Content.ReadFromJsonAsync<List<TaskItemDto>>();
        Assert.NotNull(tasks);
        Assert.Contains(tasks, t => t.Id == createdTask.Id);

        // Act - Update the task
        var updateRequest = new UpdateTaskRequest("Updated Task Title", "Updated description", "Concluida");
        var updateResponse = await client.PutAsJsonAsync($"/api/tasks/{createdTask.Id}", updateRequest);

        // Assert - Update
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updatedTask = await updateResponse.Content.ReadFromJsonAsync<TaskItemDto>();
        Assert.NotNull(updatedTask);
        Assert.Equal("Updated Task Title", updatedTask.Title);
        Assert.Equal("Updated description", updatedTask.Description);
        Assert.Equal("Concluida", updatedTask.Status);
        Assert.Equal(createdTask.Id, updatedTask.Id);

        // Act - Delete the task
        var deleteResponse = await client.DeleteAsync($"/api/tasks/{createdTask.Id}");

        // Assert - Delete
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // Verify task is deleted
        var listAfterDeleteResponse = await client.GetAsync("/api/tasks");
        var tasksAfterDelete = await listAfterDeleteResponse.Content.ReadFromJsonAsync<List<TaskItemDto>>();
        Assert.NotNull(tasksAfterDelete);
        Assert.DoesNotContain(tasksAfterDelete, t => t.Id == createdTask.Id);
    }

    /// <summary>
    /// Tests that tasks are isolated by user: each user sees only their own tasks.
    /// **Validates: Requirements 3.4, 5.1**
    /// </summary>
    [Fact]
    public async Task TasksEndpoints_IsolationByUser()
    {
        // Arrange - Register and login two different users
        var (token1, userId1) = await RegisterAndLoginAsync();
        var (token2, userId2) = await RegisterAndLoginAsync();

        var client1 = _factory.CreateClient();
        client1.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);

        var client2 = _factory.CreateClient();
        client2.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token2);

        // Act - User 1 creates a task
        var user1TaskRequest = new CreateTaskRequest("User 1 Task", "This belongs to user 1");
        var user1CreateResponse = await client1.PostAsJsonAsync("/api/tasks", user1TaskRequest);
        Assert.Equal(HttpStatusCode.Created, user1CreateResponse.StatusCode);
        var user1Task = await user1CreateResponse.Content.ReadFromJsonAsync<TaskItemDto>();
        Assert.NotNull(user1Task);

        // Act - User 2 creates a task
        var user2TaskRequest = new CreateTaskRequest("User 2 Task", "This belongs to user 2");
        var user2CreateResponse = await client2.PostAsJsonAsync("/api/tasks", user2TaskRequest);
        Assert.Equal(HttpStatusCode.Created, user2CreateResponse.StatusCode);
        var user2Task = await user2CreateResponse.Content.ReadFromJsonAsync<TaskItemDto>();
        Assert.NotNull(user2Task);

        // Act - User 1 lists their tasks
        var user1ListResponse = await client1.GetAsync("/api/tasks");
        var user1Tasks = await user1ListResponse.Content.ReadFromJsonAsync<List<TaskItemDto>>();

        // Assert - User 1 sees only their own task
        Assert.NotNull(user1Tasks);
        Assert.Contains(user1Tasks, t => t.Id == user1Task.Id);
        Assert.DoesNotContain(user1Tasks, t => t.Id == user2Task.Id);

        // Act - User 2 lists their tasks
        var user2ListResponse = await client2.GetAsync("/api/tasks");
        var user2Tasks = await user2ListResponse.Content.ReadFromJsonAsync<List<TaskItemDto>>();

        // Assert - User 2 sees only their own task
        Assert.NotNull(user2Tasks);
        Assert.Contains(user2Tasks, t => t.Id == user2Task.Id);
        Assert.DoesNotContain(user2Tasks, t => t.Id == user1Task.Id);

        // Act - User 2 tries to update User 1's task
        var updateRequest = new UpdateTaskRequest("Hacked Title", "Hacked", "Concluida");
        var hackUpdateResponse = await client2.PutAsJsonAsync($"/api/tasks/{user1Task.Id}", updateRequest);

        // Assert - User 2 cannot update User 1's task (403 Forbidden)
        Assert.Equal(HttpStatusCode.Forbidden, hackUpdateResponse.StatusCode);

        // Act - User 2 tries to delete User 1's task
        var hackDeleteResponse = await client2.DeleteAsync($"/api/tasks/{user1Task.Id}");

        // Assert - User 2 cannot delete User 1's task (403 Forbidden)
        Assert.Equal(HttpStatusCode.Forbidden, hackDeleteResponse.StatusCode);

        // Verify User 1's task is still intact
        var verifyUser1ListResponse = await client1.GetAsync("/api/tasks");
        var verifyUser1Tasks = await verifyUser1ListResponse.Content.ReadFromJsonAsync<List<TaskItemDto>>();
        Assert.NotNull(verifyUser1Tasks);
        var stillExistingTask = verifyUser1Tasks.FirstOrDefault(t => t.Id == user1Task.Id);
        Assert.NotNull(stillExistingTask);
        Assert.Equal("User 1 Task", stillExistingTask.Title); // Title unchanged
    }

    #region Helper Methods

    /// <summary>
    /// Helper method to register and login a new user, returning the JWT token and user ID.
    /// </summary>
    private async Task<(string Token, Guid UserId)> RegisterAndLoginAsync()
    {
        var username = $"user_{Guid.NewGuid():N}";
        var email = $"{username}@test.com";
        var password = "password123";

        var registerRequest = new RegisterRequest(username, email, password);
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        var loginRequest = new LoginRequest(email, password);
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var authResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(authResponse);
        Assert.NotNull(authResponse.Token);

        // Extract user ID from token
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(authResponse.Token);
        var userIdClaim = token.Claims.FirstOrDefault(c => c.Type == "sub");
        Assert.NotNull(userIdClaim);
        var userId = Guid.Parse(userIdClaim.Value);

        return (authResponse.Token, userId);
    }

    /// <summary>
    /// Helper method to generate an expired JWT token for testing.
    /// </summary>
    private string GenerateExpiredJwtToken()
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("TaskFlowSuperSecretKey12345678901234"));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("sub", Guid.NewGuid().ToString()),
            new Claim("username", "expireduser"),
            new Claim("email", "expired@test.com")
        };

        var token = new JwtSecurityToken(
            issuer: "TaskFlow",
            audience: "TaskFlow",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(-5), // Expired 5 minutes ago
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    #endregion
}
