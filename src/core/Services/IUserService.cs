namespace core.Services;
using core.Models;

public interface IUserService
{

    IEnumerable<User> GetAllJoined();
    ///<summary>
    ///Create entity and begin tracking user.
    ///Makes a trip to DB to fetch role data,
    ///dont use to consecutively add users
    ///</summary>
    Task AddUserAsync(User user);

    void RemoveUserById(uint id);

    ///<summary>
    ///Performed on main DbContext
    ///</summary>
    Task<int> SaveChangesAsync();

}
