namespace core.Services;

using core.Model;
using core.Models;

public interface IUserService
{

    IEnumerable<User> GetAllUsersJoined();
    ///<summary>
    ///Create entity and begin tracking user.
    ///Makes a trip to DB to fetch role data,
    ///dont use to consecutively add users
    ///</summary>
    Task<Result<string>> AddUserAsync(User user);

    Task RemoveUserById(uint id);

    ///<summary>
    ///Performed on main DbContext
    ///</summary>
    Task<int> SaveChangesAsync();

    Task<User> GetUserInfo(uint id);
}
