using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using core.Models;
using Data.Models;
using Data.Core;
using Microsoft.Extensions.Logging;
using Npgsql;

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

    public async Task<Result<User>> AddUserAsync(string uname,
                                                   string password,
                                                   string initiatorUname,
                                                   string? email = null,
                                                   uint? roleId = null)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(uname, nameof(uname));
        ArgumentNullException.ThrowIfNullOrEmpty(password, nameof(password));
        ArgumentNullException.ThrowIfNullOrEmpty(initiatorUname,
                                             nameof(initiatorUname));


        _logger.LogInformation($"Request to create new user from {initiatorUname}");

        bool unameExists = await _context.Users.AnyAsync(u => u.Uname == uname);
        if (unameExists)
        {
            return new Result<User>(ResultStatus.Fail,
                                      $"Username \"{uname}\" is already taken");
        }

        if (roleId is null)
        {
            return new Result<User>(ResultStatus.Fail,
                                      $"Provided role does not exist");
        }

        var userDTO = new User(uname, password, Email: email);
        var userEntity = CreateUserEntity(
                            userDTO,
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


        return new Result<User>(ResultStatus.Success,
                                resultObject: userDTO with { Id = userEntity.Id });
    }

    public async Task RemoveUserById(uint id, string issuer)
    {

        // TODO: add worker for deleting private files without owner
        // FK link is removed by pstgres
        var userIdParam = new NpgsqlParameter
        {
            ParameterName = "p_user_id",
            NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Bigint,
            Value = id
        };
        var issuerNameParam = new NpgsqlParameter
        {
            ParameterName = "p_issuer_name",
            NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Varchar,
            Value = issuer
        };

        UserEntity? deleted = await _context.Users
            .FromSqlRaw("SELECT * FROM remove_user_by_id(@p_user_id,@p_issuer_name)",
                        userIdParam,
                        issuerNameParam)
            .SingleOrDefaultAsync();

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
