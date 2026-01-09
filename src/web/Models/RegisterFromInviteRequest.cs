using System.ComponentModel.DataAnnotations;

namespace web.Models;

public record RegisterFromInviteRequest(
        [Required]
        [MinLength(2)]
        string Username,

        [Required]
        [MinLength(8)]
        string Password,
        string? Email = null);
