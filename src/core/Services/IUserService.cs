namespace core.Services;
using core.Models;

public interface IUserService
{

    IEnumerable<User> GetAllJoined();
    Task NewUserAsync(User user);
    Task<string> RemoveUserById(uint id);
    Task<int> SaveChangesAsync();

}
