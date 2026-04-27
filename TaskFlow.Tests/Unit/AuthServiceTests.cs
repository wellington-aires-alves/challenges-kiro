using Microsoft.Extensions.Configuration;
using TaskFlow.API;
using TaskFlow.Core;
using TaskFlow.Core.DTOs;
using TaskFlow.Core.Entities;
using TaskFlow.Core.Interfaces;
using TaskFlow.Core.Services;

namespace TaskFlow.Tests.Unit;

public class AuthServiceTests
{
    private readonly IConfiguration _configuration;

    public AuthServiceTests()
    {
        // Setup configuration with JWT key for testing
        var configData = new Dictionary<string, string>
        {
            { "Jwt:Key", "test-secret-key-with-at-least-32-characters-for-hmacsha256" },
            { "Jwt:Issuer", "TaskFlow" },
            { "Jwt:Audience", "TaskFlow" }
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData!)
            .Build();
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsConflictError()
    {
        // Arrange
        var userRepository = new InMemoryUserRepository();
        var authService = new AuthService(userRepository, _configuration);

        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            Username = "existinguser",
            Email = "duplicate@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
            CreatedAt = DateTime.UtcNow
        };
        await userRepository.AddAsync(existingUser);

        var request = new RegisterRequest("newuser", "duplicate@example.com", "password456");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ConflictException>(
            () => authService.RegisterAsync(request)
        );
        Assert.Equal("E-mail já cadastrado.", exception.Message);
    }

    [Fact]
    public async Task Register_WithDuplicateUsername_ReturnsConflictError()
    {
        // Arrange
        var userRepository = new InMemoryUserRepository();
        var authService = new AuthService(userRepository, _configuration);

        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            Username = "duplicateuser",
            Email = "existing@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
            CreatedAt = DateTime.UtcNow
        };
        await userRepository.AddAsync(existingUser);

        var request = new RegisterRequest("duplicateuser", "newemail@example.com", "password456");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ConflictException>(
            () => authService.RegisterAsync(request)
        );
        Assert.Equal("Nome de usuário já cadastrado.", exception.Message);
    }

    [Fact]
    public async Task Register_WithShortPassword_ReturnsValidationError()
    {
        // Arrange
        var userRepository = new InMemoryUserRepository();
        var authService = new AuthService(userRepository, _configuration);

        var request = new RegisterRequest("testuser", "test@example.com", "12345");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => authService.RegisterAsync(request)
        );
        Assert.Equal("A senha deve ter pelo menos 6 caracteres.", exception.Message);
    }

    [Fact]
    public async Task Register_WithInvalidEmail_ReturnsValidationError()
    {
        // Arrange
        var userRepository = new InMemoryUserRepository();
        var authService = new AuthService(userRepository, _configuration);

        var request = new RegisterRequest("testuser", "invalidemail", "password123");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => authService.RegisterAsync(request)
        );
        Assert.Equal("E-mail inválido.", exception.Message);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        // Arrange
        var userRepository = new InMemoryUserRepository();
        var authService = new AuthService(userRepository, _configuration);

        // Register a user first
        var registerRequest = new RegisterRequest("testuser", "test@example.com", "password123");
        await authService.RegisterAsync(registerRequest);

        // Attempt to login
        var loginRequest = new LoginRequest("test@example.com", "password123");

        // Act
        var result = await authService.LoginAsync(loginRequest);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Token);
        Assert.NotEmpty(result.Token);
        Assert.Equal("testuser", result.Username);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsAuthenticationError()
    {
        // Arrange
        var userRepository = new InMemoryUserRepository();
        var authService = new AuthService(userRepository, _configuration);

        // Register a user first
        var registerRequest = new RegisterRequest("testuser", "test@example.com", "password123");
        await authService.RegisterAsync(registerRequest);

        // Attempt to login with wrong password
        var loginRequest = new LoginRequest("test@example.com", "wrongpassword");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedException>(
            () => authService.LoginAsync(loginRequest)
        );
        Assert.Equal("Credenciais inválidas.", exception.Message);
    }

    [Fact]
    public async Task Login_WithUnregisteredEmail_ReturnsAuthenticationError()
    {
        // Arrange
        var userRepository = new InMemoryUserRepository();
        var authService = new AuthService(userRepository, _configuration);

        var loginRequest = new LoginRequest("nonexistent@example.com", "password123");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedException>(
            () => authService.LoginAsync(loginRequest)
        );
        Assert.Equal("Credenciais inválidas.", exception.Message);
    }

    [Fact]
    public async Task Login_WithBlankFields_ReturnsValidationError()
    {
        // Arrange
        var userRepository = new InMemoryUserRepository();
        var authService = new AuthService(userRepository, _configuration);

        // Test with blank email
        var loginRequestBlankEmail = new LoginRequest("", "password123");
        var exceptionEmail = await Assert.ThrowsAsync<ValidationException>(
            () => authService.LoginAsync(loginRequestBlankEmail)
        );
        Assert.Equal("E-mail é obrigatório.", exceptionEmail.Message);

        // Test with blank password
        var loginRequestBlankPassword = new LoginRequest("test@example.com", "");
        var exceptionPassword = await Assert.ThrowsAsync<ValidationException>(
            () => authService.LoginAsync(loginRequestBlankPassword)
        );
        Assert.Equal("Senha é obrigatória.", exceptionPassword.Message);
    }
}
