namespace core.Models;

public record UserCreationDto(
    string Username,
    string Password,
    string CreatedBy,
    byte RoleId,
    string? Email
);
