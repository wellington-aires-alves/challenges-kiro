using FsCheck;
using FsCheck.Xunit;
using Microsoft.Extensions.Configuration;
using TaskFlow.API;
using TaskFlow.Core;
using TaskFlow.Core.DTOs;
using TaskFlow.Core.Entities;
using TaskFlow.Core.Interfaces;
using TaskFlow.Core.Services;

namespace TaskFlow.Tests.Unit;

/// <summary>
/// Property-based tests for AuthService
/// </summary>
public class AuthServicePropertyTests
{
    private readonly IConfiguration _configuration;

    public AuthServicePropertyTests()
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

    // Feature: taskflow, Property 1: Unicidade de e-mail no cadastro
    /// <summary>
    /// **Validates: Requirements 1.3**
    /// 
    /// Property: For any email already present in the repository, a second registration attempt
    /// with the same email should be rejected and the repository should remain unchanged.
    /// 
    /// This test runs 100 iterations with randomly generated valid user data to verify
    /// that duplicate email registration is always rejected regardless of the specific values.
    /// </summary>
    [Fact]
    public async Task Register_DuplicateEmail_AlwaysRejects()
    {
        var random = new Random(42); // Fixed seed for reproducibility
        var failedIterations = new List<string>();

        // Run 100 iterations as specified in the design document
        for (int i = 0; i < 100; i++)
        {
            try
            {
                // Generate random valid user data
                var username1 = $"user_{random.Next(1000000)}";
                var username2 = $"user_{random.Next(1000000)}";
                var email = $"test{random.Next(1000000)}@example.com";
                var password1 = $"Pass{random.Next(1000000)}!";
                var password2 = $"Pass{random.Next(1000000)}!";

                // Ensure usernames are different
                while (username1 == username2)
                {
                    username2 = $"user_{random.Next(1000000)}";
                }

                // Arrange
                var userRepository = new InMemoryUserRepository();
                var authService = new AuthService(userRepository, _configuration);

                // Register the first user
                var firstRequest = new RegisterRequest(username1, email, password1);
                var firstResult = await authService.RegisterAsync(firstRequest);
                
                Assert.True(firstResult.Success, $"Iteration {i}: First registration should succeed");

                // Get the first user to verify state
                var firstUser = await userRepository.GetByEmailAsync(email);
                Assert.NotNull(firstUser);

                // Create a second request with the SAME email but different username and password
                var secondRequest = new RegisterRequest(username2, email, password2);

                // Act & Assert
                var exception = await Assert.ThrowsAsync<ConflictException>(
                    () => authService.RegisterAsync(secondRequest)
                );

                // Verify the error message
                Assert.Equal("E-mail já cadastrado.", exception.Message);

                // Verify repository remains unchanged - the original user is still there
                var userAfter = await userRepository.GetByEmailAsync(email);
                Assert.NotNull(userAfter);
                Assert.Equal(firstUser.Id, userAfter.Id);
                Assert.Equal(firstUser.Username, userAfter.Username);
                Assert.Equal(firstUser.Email, userAfter.Email);
                Assert.Equal(firstUser.PasswordHash, userAfter.PasswordHash);

                // Verify the second username was NOT added
                var secondUser = await userRepository.GetByUsernameAsync(username2);
                Assert.Null(secondUser);
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

    // Feature: taskflow, Property 2: Unicidade de nome de usuário no cadastro
    /// <summary>
    /// **Validates: Requirements 1.4**
    /// 
    /// Property: For any username already present in the repository, a second registration attempt
    /// with the same username should be rejected and the repository should remain unchanged.
    /// 
    /// This test runs 100 iterations with randomly generated valid user data to verify
    /// that duplicate username registration is always rejected regardless of the specific values.
    /// </summary>
    [Fact]
    public async Task Register_DuplicateUsername_AlwaysRejects()
    {
        var random = new Random(43); // Fixed seed for reproducibility (different from email test)
        var failedIterations = new List<string>();

        // Run 100 iterations as specified in the design document
        for (int i = 0; i < 100; i++)
        {
            try
            {
                // Generate random valid user data
                var username = $"user_{random.Next(1000000)}";
                var email1 = $"test{random.Next(1000000)}@example.com";
                var email2 = $"test{random.Next(1000000)}@example.com";
                var password1 = $"Pass{random.Next(1000000)}!";
                var password2 = $"Pass{random.Next(1000000)}!";

                // Ensure emails are different
                while (email1 == email2)
                {
                    email2 = $"test{random.Next(1000000)}@example.com";
                }

                // Arrange
                var userRepository = new InMemoryUserRepository();
                var authService = new AuthService(userRepository, _configuration);

                // Register the first user
                var firstRequest = new RegisterRequest(username, email1, password1);
                var firstResult = await authService.RegisterAsync(firstRequest);
                
                Assert.True(firstResult.Success, $"Iteration {i}: First registration should succeed");

                // Get the first user to verify state
                var firstUser = await userRepository.GetByUsernameAsync(username);
                Assert.NotNull(firstUser);

                // Create a second request with the SAME username but different email and password
                var secondRequest = new RegisterRequest(username, email2, password2);

                // Act & Assert
                var exception = await Assert.ThrowsAsync<ConflictException>(
                    () => authService.RegisterAsync(secondRequest)
                );

                // Verify the error message
                Assert.Equal("Nome de usuário já cadastrado.", exception.Message);

                // Verify repository remains unchanged - the original user is still there
                var userAfter = await userRepository.GetByUsernameAsync(username);
                Assert.NotNull(userAfter);
                Assert.Equal(firstUser.Id, userAfter.Id);
                Assert.Equal(firstUser.Username, userAfter.Username);
                Assert.Equal(firstUser.Email, userAfter.Email);
                Assert.Equal(firstUser.PasswordHash, userAfter.PasswordHash);

                // Verify the second email was NOT added
                var secondUser = await userRepository.GetByEmailAsync(email2);
                Assert.Null(secondUser);
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

    // Feature: taskflow, Property 3: Senha nunca armazenada em texto simples
    /// <summary>
    /// **Validates: Requirements 1.7**
    /// 
    /// Property: For any password provided during registration, the stored value in the repository
    /// should be different from the original password AND should be verifiable by BCrypt.
    /// 
    /// This test runs 100 iterations with randomly generated passwords to verify
    /// that passwords are always hashed and never stored in plain text.
    /// </summary>
    [Fact]
    public async Task Register_PasswordAlwaysStoredAsHash()
    {
        var random = new Random(44); // Fixed seed for reproducibility
        var failedIterations = new List<string>();

        // Run 100 iterations as specified in the design document
        for (int i = 0; i < 100; i++)
        {
            try
            {
                // Generate random valid user data with varying password patterns
                var username = $"user_{random.Next(1000000)}";
                var email = $"test{random.Next(1000000)}@example.com";
                
                // Generate diverse passwords: short, long, with special chars, numbers, etc.
                var passwordPatterns = new[]
                {
                    $"Pass{random.Next(1000000)}!",           // alphanumeric + special
                    $"simple{random.Next(100)}",              // simple alphanumeric
                    $"VeryLongPassword{random.Next(1000000)}WithManyCharacters!@#$%", // long password
                    $"123456{random.Next(100)}",              // numeric heavy
                    $"!@#$%^{random.Next(100)}",              // special char heavy
                    $"MixedCase{random.Next(1000)}LowerUpper" // mixed case
                };
                
                var password = passwordPatterns[i % passwordPatterns.Length];

                // Arrange
                var userRepository = new InMemoryUserRepository();
                var authService = new AuthService(userRepository, _configuration);

                // Act - Register the user
                var request = new RegisterRequest(username, email, password);
                var result = await authService.RegisterAsync(request);
                
                Assert.True(result.Success, $"Iteration {i}: Registration should succeed");

                // Get the stored user from repository
                var storedUser = await userRepository.GetByEmailAsync(email);
                Assert.NotNull(storedUser);

                // Assert 1: The stored PasswordHash must NOT equal the original password
                Assert.NotEqual(password, storedUser.PasswordHash);
                
                // Assert 2: The stored PasswordHash must be verifiable by BCrypt
                var isValidHash = BCrypt.Net.BCrypt.Verify(password, storedUser.PasswordHash);
                Assert.True(isValidHash, 
                    $"Iteration {i}: BCrypt should be able to verify the stored hash against the original password");

                // Assert 3: The stored hash should look like a BCrypt hash (starts with $2a$, $2b$, or $2y$)
                Assert.Matches(@"^\$2[aby]\$\d{2}\$.{53}$", storedUser.PasswordHash);

                // Assert 4: Verify that a wrong password does NOT verify against the hash
                var wrongPassword = password + "wrong";
                var isWrongPasswordValid = BCrypt.Net.BCrypt.Verify(wrongPassword, storedUser.PasswordHash);
                Assert.False(isWrongPasswordValid, 
                    $"Iteration {i}: A wrong password should not verify against the stored hash");
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

    // Feature: taskflow, Property 4: Credenciais inválidas não emitem token
    /// <summary>
    /// **Validates: Requirements 2.3, 2.4**
    /// 
    /// Property: For any combination of email and password where the password does not match
    /// the stored hash for that email, the authentication service should NOT emit a JWT token.
    /// 
    /// This test runs 100 iterations with randomly generated user data and wrong passwords
    /// to verify that invalid credentials never result in token emission.
    /// </summary>
    [Fact]
    public async Task Login_InvalidCredentials_NeverEmitsToken()
    {
        var random = new Random(45); // Fixed seed for reproducibility
        var failedIterations = new List<string>();

        // Run 100 iterations as specified in the design document
        for (int i = 0; i < 100; i++)
        {
            try
            {
                // Generate random valid user data
                var username = $"user_{random.Next(1000000)}";
                var email = $"test{random.Next(1000000)}@example.com";
                var correctPassword = $"Pass{random.Next(1000000)}!";

                // Arrange - Register a user first
                var userRepository = new InMemoryUserRepository();
                var authService = new AuthService(userRepository, _configuration);

                var registerRequest = new RegisterRequest(username, email, correctPassword);
                var registerResult = await authService.RegisterAsync(registerRequest);
                
                Assert.True(registerResult.Success, $"Iteration {i}: Registration should succeed");

                // Generate various types of wrong passwords (all non-blank to avoid ValidationException)
                var wrongPasswordPatterns = new[]
                {
                    correctPassword + "wrong",                    // Append text
                    "wrong" + correctPassword,                    // Prepend text
                    correctPassword.ToUpper(),                    // Change case
                    correctPassword.ToLower(),                    // Change case
                    correctPassword.Substring(0, Math.Max(1, correctPassword.Length - 1)), // Truncate
                    correctPassword + random.Next(100),           // Append number
                    $"WrongPass{random.Next(1000000)}!",         // Completely different
                    $"Different{random.Next(1000)}",              // Different password
                    $"Another{random.Next(1000)}Pass",            // Another different password
                    new string(correctPassword.Reverse().ToArray()) // Reversed
                };

                var wrongPassword = wrongPasswordPatterns[i % wrongPasswordPatterns.Length];

                // Act - Attempt login with wrong password
                var loginRequest = new LoginRequest(email, wrongPassword);

                // Assert - Should throw UnauthorizedException (not emit token)
                var exception = await Assert.ThrowsAsync<UnauthorizedException>(
                    () => authService.LoginAsync(loginRequest)
                );

                // Verify the error message
                Assert.Equal("Credenciais inválidas.", exception.Message);

                // Additional verification: Try to login with correct password to ensure user is still valid
                var validLoginRequest = new LoginRequest(email, correctPassword);
                var validLoginResult = await authService.LoginAsync(validLoginRequest);
                
                Assert.True(validLoginResult.Success, 
                    $"Iteration {i}: Login with correct password should succeed after failed attempt");
                Assert.NotNull(validLoginResult.Token);
                Assert.NotEmpty(validLoginResult.Token);
                Assert.Equal(username, validLoginResult.Username);

                // Test with non-existent email as well (Requirements 2.3)
                var nonExistentEmail = $"nonexistent{random.Next(1000000)}@example.com";
                var nonExistentLoginRequest = new LoginRequest(nonExistentEmail, correctPassword);
                
                var nonExistentException = await Assert.ThrowsAsync<UnauthorizedException>(
                    () => authService.LoginAsync(nonExistentLoginRequest)
                );
                
                Assert.Equal("Credenciais inválidas.", nonExistentException.Message);
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
