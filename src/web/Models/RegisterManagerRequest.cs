using System.ComponentModel.DataAnnotations;

namespace web.Models;

public record RegisterManagerRequest(
        [Required] string Username,
        [Required] string Password,
        [Required] byte Role,
        string Email);
