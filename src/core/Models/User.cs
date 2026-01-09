namespace core.Models;
using Data.Models;

// It does not really need to be mutable, but,
// just for the sake of core.Services.AuthService.Authenticate(), it is
//public required string Uname { get; init;}
//public string? Password { get; set; }
//public string? Role { get; set; }
//public uint Id { get; set; } = 0;

//public User(string uname, string? password = null, string? role = null, uint id = 0)
//{
//    Uname = uname;
//    Password = password;
//    Role = role;
//    Id = id;
//}
public record User(string? Uname, string? Password = null, string? Role = null, uint? Id = null, string? Email = null)
{
    static public User FromEntity(UserEntity entity) => new User(entity.Uname,
                                        Role: entity.Role?.Name ?? string.Empty,
                                        Email: entity.Email ?? string.Empty,
                                        Id: entity.Id);
}
//
//}
