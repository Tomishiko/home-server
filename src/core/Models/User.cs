namespace core.Models;

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
    public record User(string Uname, string Password,string? Role,uint Id = 0);
    //
    //}
