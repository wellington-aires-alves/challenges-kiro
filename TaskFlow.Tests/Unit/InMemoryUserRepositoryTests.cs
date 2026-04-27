using TaskFlow.API;
using TaskFlow.Core.Entities;

namespace TaskFlow.Tests.Unit;

public class InMemoryUserRepositoryTests
{
    [Fact]
    public async Task Add_AndGetById_ReturnsAddedItem()
    {
        // Arrange
        var repository = new InMemoryUserRepository();
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hashedpassword",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        await repository.AddAsync(user);
        var result = await repository.GetByIdAsync(user.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal(user.Username, result.Username);
        Assert.Equal(user.Email, result.Email);
        Assert.Equal(user.PasswordHash, result.PasswordHash);
    }
}
