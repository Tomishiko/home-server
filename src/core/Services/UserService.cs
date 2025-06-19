namespace core.Services;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using core.Models;
using Data.Models;
using Data.Shared;
using Data.Core;


public class UserService : BaseDataService, IUserService
{
    IPasswordHasher<User> _hasher;

    public UserService(IPasswordHasher<User> hasher, ApplicationDbContext context) : base(context)
    {
        _hasher = hasher;
    }

    public IEnumerable<User> GetAllJoined()
    {
        return _context.Users
                .Include("Role")
                .Select(u => new User(u.Uname, string.Empty, u.Role.Name, u.Id));
    }
    public async Task NewUserAsync(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var roleId = await _context.Roles
                                    .Where(r => r.Name == user.Role)
                                    .Select(r => r.Id)
                                    .SingleAsync();

        var userEntity = CreateEntity(user);
        userEntity.role_id = roleId;
        //TODO: fix reporting the count of changes
        await _context.Users.AddAsync(userEntity);
    }

    public async Task<string> RemoveUserById(uint id)
    {
        var userEntity = await _context.Users.FindAsync(id);
        if (userEntity == null)
            throw new ArgumentOutOfRangeException(nameof(id), "No user found with provided Id");

        _context.Users.Remove(userEntity);
        //TODO: fix reporting the count of changes
        return userEntity.Uname;

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
