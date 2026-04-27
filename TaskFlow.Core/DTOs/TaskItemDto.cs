namespace TaskFlow.Core.DTOs;

public record TaskItemDto(Guid Id, string Title, string? Description, string Status, DateTime CreatedAt);
