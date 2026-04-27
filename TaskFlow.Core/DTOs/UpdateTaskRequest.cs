namespace TaskFlow.Core.DTOs;

public record UpdateTaskRequest(string Title, string? Description, string Status);
