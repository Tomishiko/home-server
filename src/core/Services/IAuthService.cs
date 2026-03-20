using core.Models;
using core.Models.Generic;

namespace core.Services;

public interface IAuthService
{

    ///<summary>Checks if the user is in DB</summary>
    ///<param name="user">Contains info about user,fields are updated from DB if auth is succesfull</param>
    ///<returns>True if user is in DB and passwords match, false othervise</returns>
    ///<exception cref="ArgumentNullException">Thrown when <paramref name="dto"/> is null</exception>
    ///<exception cref="InvalidOperationException">Thrown when there is more then one corresponding usernames</exception>
    Task<Result<UserDto>> AuthenticateAsync(UserAuthDto dto);
}
