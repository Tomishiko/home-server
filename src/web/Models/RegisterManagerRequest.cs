using System.ComponentModel.DataAnnotations;

namespace web.Models;

public record RegisterManagerRequest(
        [Required] string Username,
        [Required] string Password,
        [Required] uint Role,
        string Email);
