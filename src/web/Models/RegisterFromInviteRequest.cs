using System.ComponentModel.DataAnnotations;

namespace web.Models;

public record RegisterFromInviteRequest(
    [Required(ErrorMessage = "Username is required")]
    [RegularExpression(@"^[a-zA-Z][a-zA-Z0-9_]{2,15}$",
    ErrorMessage = "Username must start with a letter, be 3-16 characters long, and contain only letters, numbers, or underscores.")]

    string Username,

    [Required(ErrorMessage = "Password is required")]
    [StringLength(32, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 32 characters")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
    ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character (@, $, !, %, *, ?, &).")]

    string Password,

    [EmailAddress(ErrorMessage = "Invalid email format")]
    [MaxLength(254, ErrorMessage = "Email cannot exceed 254 characters")]

    string? Email = null
);
