namespace TaskFlow.Core.DTOs;

public record AuthResult(bool Success, string? Token, string? Username, string? ErrorMessage);
