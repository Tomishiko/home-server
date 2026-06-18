namespace core.Models;

public sealed record UserDto(string Username,
                             string Role,
                             long Id = -1,
                             string? Email = null);


