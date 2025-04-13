namespace core.Services;
using core.Models;

public interface IUserService
{
    IEnumerable<User> GetAll();
    Task NewUserAsync(User user);
    void NewUser(User user);
    Task<bool> RemoveUser(uint id);
    int SaveChanges();
    Task<int> SaveChangesAsync();

}
