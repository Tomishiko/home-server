using core.Domain;
using core.Models;
using core.Models.Generic;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace core.Services;


public interface IUserService : IBaseDataService
{

    IAsyncEnumerable<UserDto> GetAllUsersJoinedAsync(CancellationToken ct = default);
    ///<summary>
    ///Create entity and begin tracking user.
    ///Makes a trip to DB to fetch role data,
    ///dont use to consecutively add users
    ///</summary>
    Task<Result<UserDto>> AddUserAsync(UserCreationDto dto);

    Task<Result<EntityEntry<UserEntity>>> StageNewUser(UserCreationDto dto);

    Task<Result<UserDto>> RemoveUserById(long id, string issuer);

    Task<UserDto?> GetUserInfo(long id);
}
