using System.ComponentModel.DataAnnotations;

namespace web.Models.RequestModels;

public sealed record AuthRequest
{
    [Required]
    [StringLength(256)]
    [MinLength(3)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MinLength(5)]
    public string Password { get; set; } = string.Empty;

}
