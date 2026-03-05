namespace core.Domain;

public class UserEntity : BaseEntity
{
    public string Uname { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string? Email { get; set; }

    public long RoleId { get; set; }

    public RolesEntity? Role { get; set; }
}
