using core.Domain;

namespace core.Models;

public static class UserMapper
{
    ///<summary>Allocates new dto from entity</summary>
    public static UserDto ToDto(this UserEntity entity)
    {

        return new UserDto(Username: entity.Uname,
                           Role: entity.Role?.Name ?? string.Empty,
                           Email: entity.Email ?? string.Empty,
                           Id: entity.Id);
    }
}
