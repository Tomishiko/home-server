using core.Models;
using core.Models.Generic;

namespace core.Services;


public interface IUserService : IBaseDataService
{

    IEnumerable<UserDto> GetAllUsersJoined();
    ///<summary>
    ///Create entity and begin tracking user.
    ///Makes a trip to DB to fetch role data,
    ///dont use to consecutively add users
    ///</summary>
    Task<Result<UserDto>> AddUserAsync(UserCreationDto dto);

    Task<Result<UserDto>> RemoveUserById(long id, string issuer);

    Task<UserDto?> GetUserInfo(long id);
}
