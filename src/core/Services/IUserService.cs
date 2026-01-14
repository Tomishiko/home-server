using core.Models;

namespace core.Services;


public interface IUserService : IBaseDataService
{

    IEnumerable<User> GetAllUsersJoined();
    ///<summary>
    ///Create entity and begin tracking user.
    ///Makes a trip to DB to fetch role data,
    ///dont use to consecutively add users
    ///</summary>
    Task<Result<User>> AddUserAsync(string uname,
                                    string password,
                                    string initiatorUname,
                                    string? email = null,
                                    uint? role = null);

    Task RemoveUserById(uint id, string issuer);

    Task<User?> GetUserInfo(uint id);
}
