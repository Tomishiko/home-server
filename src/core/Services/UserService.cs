namespace core.Services;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using core.Models;
using Data.Models;
using Data.Shared;


public class UserService : IUserService
{
    IRepository<UserEntity> _userRepo;
    IRepository<LogsEntity> _logRepo;
    IRepository<RolesEntity> _roleRepo;
    IPasswordHasher<User> _hasher;

    public UserService(IRepository<UserEntity> userRepo, IRepository<LogsEntity> logRepo, IPasswordHasher<User> hasher, IRepository<RolesEntity> roleRepo)
    {
        _userRepo = userRepo;
        _logRepo = logRepo;
        _hasher = hasher;
        _roleRepo = roleRepo;
    }

    public IEnumerable<User> GetAll()
    {
        return _userRepo.Query()
                .Include("Role")
                .Select(u => new User(u.Uname, string.Empty, u.Role.Name, u.Id));
    }
    public async Task NewUserAsync(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var roleId = await _roleRepo.Query()
                                    .Where(r => r.Name == user.Role)
                                    .Select(r => r.Id)
                                    .SingleAsync();

        var userEntity = CreateEntity(user);
        userEntity.role_id = roleId;
        //TODO: fix reporting the count of changes
        await _userRepo.AddAsync(userEntity);
    }

    public void NewUser(User user)
    {
        ArgumentNullException.ThrowIfNull(user, nameof(user));
        var userEntity = CreateEntity(user);
        _userRepo.Add(userEntity);

    }

    public async Task<bool> RemoveUser(uint id)
    {
        var userEntity = await _userRepo.GetByIdAsync(id);
        if (userEntity == null)
            throw new ArgumentOutOfRangeException(nameof(id), "No user found with provided Id");

        var logs = await _logRepo.Query()
            .Where(l => l.user_id == id)
            .ExecuteUpdateAsync(setter => setter.SetProperty(l => l.user_id, (uint?)null));


        _userRepo.Delete(userEntity);
        //TODO: fix reporting the count of changes
        await _userRepo.SaveContextAsync();
        return true;

    }
    public int SaveChanges()
    {
        return _userRepo.SaveContext();
    }
    public Task<int> SaveChangesAsync()
    {
        return _userRepo.SaveContextAsync();
    }

    private UserEntity CreateEntity(User user)
    {
        var hashed = _hasher.HashPassword(user, user.Password);
        return new UserEntity()
        {
            Uname = user.Uname,
            Password = hashed
        };
    }
}
