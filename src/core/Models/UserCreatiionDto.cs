namespace core.Models;

public readonly record struct UserCreationDto(
    string Username,
    string Password,
    string CreatedBy,
    byte RoleId,
    string? Email
);
