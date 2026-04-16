using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using core.Models;
using core.Domain;
using core.Interfaces;
using core.Models.Generic;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace core.Services;


/// <inheritdoc />
public class UserService : BaseDataService, IUserService
{
    private readonly IPasswordHasher<UserEntity> _hasher;
    private readonly ILogger<UserService> _logger;

    public UserService(IPasswordHasher<UserEntity> hasher,
                       IApplicationDbContext context,
                       ILogger<UserService> logger) : base(context)
    {
        _hasher = hasher;
        _logger = logger;
    }

    public async IAsyncEnumerable<UserDto> GetAllUsersJoinedAsync([EnumeratorCancellation] CancellationToken ct = default)
    {
        var stream = _context.Users
                    .Include("Role")
                    .AsNoTracking()
                    .AsAsyncEnumerable()
                    .WithCancellation(ct);

        await foreach (var user in stream)
        {
            yield return user.ToDto();
        }
    }

    public Task<UserDto?> GetUserInfo(long id) =>
        _context.Users.Where(u => u.Id == id)
                      .Include("Role")
                      .AsNoTracking()
                      .Select(u => u.ToDto())
                      .SingleOrDefaultAsync();

    public async Task<Result<UserDto>> AddUserAsync(UserCreationDto dto)
    {
        ArgumentException.ThrowIfNullOrEmpty(dto.Username);
        ArgumentException.ThrowIfNullOrEmpty(dto.Password);
        ArgumentException.ThrowIfNullOrEmpty(dto.CreatedBy);
        ArgumentNullException.ThrowIfNull(dto.RoleId);


        _logger.LogInformation($"Request to create new user from {dto.CreatedBy}");

        bool unameExists = await _context.Users.AnyAsync(u => u.Uname == dto.Username);
        if (unameExists)
        {
            return new Error($"Username \"{dto.Username}\" is already taken");
        }

        var userEntity = new UserEntity()
        {
            Uname = dto.Username,
            Email = dto.Email,
            RoleId = dto.RoleId
        };
        userEntity.Password = _hasher.HashPassword(userEntity, dto.Password);

        var logEntity = new LogsEntity
        {
            Time = DateTime.UtcNow,
            Uname = dto.CreatedBy,
            Event = $"New user added {userEntity.Uname}"
        };

        _context.Logs.Add(logEntity);
        _context.Users.Add(userEntity);
        await SaveChangesAsync();


        return new Success<UserDto>(userEntity.ToDto());
    }

    public async Task<Result<UserDto>> RemoveUserById(long id, string issuer)
    {

        var result = await _context.RemoveUserByIdStoredProcAsync(id, issuer);
        if (result is null)
        {
            return new Error("No such user");
        }
        return new Success<UserDto>(result.ToDto());

    }


}
