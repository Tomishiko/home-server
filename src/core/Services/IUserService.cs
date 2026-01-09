using core.Models;

namespace core.Services;


public interface IUserService
{

    IEnumerable<User> GetAllUsersJoined();
    ///<summary>
    ///Create entity and begin tracking user.
    ///Makes a trip to DB to fetch role data,
    ///dont use to consecutively add users
    ///</summary>
    Task<Result<string>> AddUserAsync(string uname,
                                                   string password,
                                                   string initiatorUname,
                                                   string? email = null,
                                                   uint? role = null);

    Task RemoveUserById(uint id);

    ///<summary>
    ///Performed on main DbContext
    ///</summary>
    Task<int> SaveChangesAsync();

    Task<User?> GetUserInfo(uint id);
}
