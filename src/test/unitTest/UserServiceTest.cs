using core.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using core.Domain;
using Data.Core;
using core.Models;
namespace UnitTest;

public class UserServiceTests
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<IPasswordHasher<UserEntity>> _mockHasher;
    private readonly Mock<ILogger<UserService>> _mockLogger;
    private readonly IUserService _service;

    public UserServiceTests()
    {

        var moqContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
        _context = moqContext.Object;
        _mockHasher = new Mock<IPasswordHasher<UserEntity>>();
        _mockLogger = new Mock<ILogger<UserService>>();

        _service = new UserService(_mockHasher.Object, _context, _mockLogger.Object);

    }
    [Fact]
    public async Task AddUserShouldTrhowForNullData()
    {
        IAuthService service = new AuthService(_context);
        var user = new UserDto("hello");
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await service.AuthenticateAsync(user));
    }

    // [Fact]
    // public async Task AddUserAsync_ShouldReturnSuccess_WhenDataIsValid()
    // {
    //     // Arrange
    //     var uname = "newuser";
    //     var pwd = "Password123!";
    //     var initiator = "admin";
    //     _mockHasher.Setup(h => h.HashPassword(It.IsAny<User>(), pwd))
    //                .Returns("hashed_password");

    //     // Act
    //     var result = await _service.AddUserAsync(uname, pwd, initiator, "test@test.com", 1);

    //     // Assert
    //     Assert.Equal(ResultStatus.Success, result.Status);
    //     Assert.True(await _context.Users.AnyAsync(u => u.Uname == uname));
    //     Assert.True(await _context.Logs.AnyAsync(l => l.Uname == initiator));
    // }

    // [Fact]
    // public async Task AddUserAsync_ShouldFail_WhenUsernameAlreadyExists()
    // {
    //     // Arrange
    //     var existingUname = "existing";
    //     _context.Users.Add(new UserEntity { Uname = existingUname, Password = "...", role_id = 1 });
    //     await _context.SaveChangesAsync();

    //     // Act
    //     var result = await _service.AddUserAsync(existingUname, "password", "admin");

    //     // Assert
    //     Assert.Equal(ResultStatus.Fail, result.Status);
    //     Assert.Contains("already taken", result.Message);
    // }

    // [Fact]
    // public async Task AddUserAsync_ShouldFail_WhenRoleDoesNotExist()
    // {
    //     // Act - Using a role ID (99) that wasn't seeded
    //     var result = await _service.AddUserAsync("user", "pass", "admin", role: 99);

    //     // Assert
    //     Assert.Equal(ResultStatus.Fail, result.Status);
    //     Assert.Equal("Provided role does not exist", result.Message);
    // }

    // [Fact]
    // public async Task GetUserInfo_ShouldReturnUser_WhenIdExists()
    // {
    //     // Arrange
    //     var user = new UserEntity { Id = 10, Uname = "findme", role_id = 1 };
    //     _context.Users.Add(user);
    //     await _context.SaveChangesAsync();

    //     // Act
    //     var result = await _service.GetUserInfo(10);

    //     // Assert
    //     Assert.NotNull(result);
    //     Assert.Equal("findme", result.Uname);
    // }

    // [Fact]
    // public async Task RemoveUserById_ShouldDeleteUserAndUnlinkFiles()
    // {
    //     // Arrange
    //     var userId = 5u;
    //     _context.Users.Add(new UserEntity { Id = userId, Uname = "deleteme", role_id = 1 });
    //     _context.Files.Add(new FileEntity { Id = 1, owner_id = userId });
    //     await _context.SaveChangesAsync();

    //     // Act
    //     await _service.RemoveUserById(userId);

    //     // Assert
    //     var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
    //     var file = await _context.Files.FindAsync(1u);

    //     Assert.False(userExists);
    //     Assert.Null(file!.owner_id); // Verify file link was removed (set to null)
    // }

}
