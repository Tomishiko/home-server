using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using core.Models;
using Data.Models;
using Data.Core;
using Microsoft.Extensions.Logging;

namespace core.Services;


/// <inheritdoc />
public class UserService : BaseDataService, IUserService
{
    private readonly IPasswordHasher<User> _hasher;
    private readonly ILogger<UserService> _logger;

    public UserService(IPasswordHasher<User> hasher,
                       ApplicationDbContext context,
                       ILogger<UserService> logger) : base(context)
    {
        _hasher = hasher;
        _logger = logger;
    }

    public IEnumerable<User> GetAllUsersJoined()
    {
        return _context.Users
                .Include("Role")
                .Select(u => User.FromEntity(u));
    }

    public Task<User?> GetUserInfo(uint id) =>
        _context.Users.Where(u => u.Id == id)
                      .Include("Role")
                      .Select(u => User.FromEntity(u))
                      .SingleOrDefaultAsync();

    public async Task<Result<string>> AddUserAsync(string uname,
                                                   string password,
                                                   string initiatorUname,
                                                   string? email = null,
                                                   uint? role = null)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(uname, nameof(uname));
        ArgumentNullException.ThrowIfNullOrEmpty(password, nameof(password));
        ArgumentNullException.ThrowIfNullOrEmpty(initiatorUname,
                                             nameof(initiatorUname));


        _logger.LogInformation($"Request to create new user from {initiatorUname}");

        bool unameExists = await _context.Users.AnyAsync(u => u.Uname == uname);
        if (unameExists)
        {
            return new Result<string>(ResultStatus.Fail,
                                      $"Username \"{uname}\" is already taken");
        }

        uint? roleId = await _context.Roles
                                    .Where(r => r.Id == (role ?? (uint)Roles.Default)) //It is what it is
                                    .Select(r => (uint?)r.Id)
                                    .SingleOrDefaultAsync();
        if (roleId is null)
        {
            return new Result<string>(ResultStatus.Fail,
                                      $"Provided role does not exist");
        }

        var userEntity = CreateUserEntity(
                            new User(uname, password, Email: email),
                            (uint)roleId);
        var logEntity = new LogsEntity
        {
            Time = DateTime.UtcNow,
            Uname = initiatorUname,
            Event = $"New user added {userEntity.Uname}"
        };

        //TODO: fix reporting the count of changes
        _context.Logs.Add(logEntity);
        _context.Users.Add(userEntity);
        await SaveChangesAsync();

        return new Result<string>(ResultStatus.Success, null);
    }

    public async Task RemoveUserById(uint id)
    {
        // Remove file link
        await _context.Files
            .Where(f => f.owner_id == id)
            .ExecuteUpdateAsync(setters => setters.SetProperty(f => f.owner_id,
                                                               (uint?)null));

        await _context.Users.Where(u => u.Id == id).ExecuteDeleteAsync();
    }

    private UserEntity CreateUserEntity(User user, uint roleId)
    {
        var hashed = _hasher.HashPassword(user, user.Password!);
        return new UserEntity()
        {
            Uname = user.Uname!,
            Password = hashed,
            Email = user.Email,
            role_id = roleId
        };
    }


}
