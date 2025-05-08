namespace core.Models;

public record User(string Uname, string Password,string? Role,uint Id = 0);
