namespace core.Services;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using core.Models;
using Data.Models;
using Data.Core;
using core.Model;

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

    public async Task<Result<string>> AddUserAsync(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var userEntity = CreateEntity(user);
        if (await _context.Users.AnyAsync(u => u.Uname == user.Uname))
        {
            return new Result<string>(ResultStatus.Fail, $"Username \"{user.Uname}\" is already taken");
        }
        var roleId = await _context.Roles
                                    .Where(r => r.Name == user.Role)
                                    .Select(r => r.Id)
                                    .SingleAsync();


        userEntity.role_id = roleId;
        //TODO: fix reporting the count of changes
        await _context.Users.AddAsync(userEntity);
        return new Result<string>(ResultStatus.Success, null);
    }

    public async Task RemoveUserById(uint id)
    {
        //var userEntity = await _context.Users.FindAsync(id);
        //if (userEntity == null)
        //    throw new ArgumentOutOfRangeException(nameof(id), "No user found with provided Id");
        await _context.Files.Where(f => f.owner_id == id)
            .ForEachAsync(f =>
            {
                if (f.Private)
                    _context.Remove(f);
                else
                {
                    f.owner_id = null;
                    _context.Update(f);
                }
            });

        var entity = new UserEntity { Id = id };
        _context.Users.Remove(entity);
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
