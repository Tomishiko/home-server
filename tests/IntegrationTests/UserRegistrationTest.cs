using core.Domain;
using core.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tests.Integration.Infra;
using web.Models;
using Xunit.Abstractions;
using FluentAssertions.Web;

namespace Tests.Integration;


[Collection("Database collection")]
public sealed class UserRegistrationTest : IClassFixture<WebAppFactory>, IAsyncLifetime
{
    private readonly WebAppFactory _factory;
    private readonly ITestOutputHelper _output;

    public UserRegistrationTest(WebAppFactory factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
        _factory.Output = output;
    }

    [Fact]
    public async Task NewUserIsAddedToDb()
    {

        var client = _factory.CreateClient();
        var body = new RegisterManagerRequest
        {
            Email = "test@email",
            Password = "Testpassword1!",
            Role = 1,
            Username = "testusername"
        };
        var response = await client.PostAsJsonAsync("/api/user", body);
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var user = await db.Users.Where(u => u.Uname == body.Username)
                                 .SingleOrDefaultAsync();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<UserEntity>>();

        response.Should().Be201Created();
        user.Should().NotBeNull();
        user.Uname.Should().Be(body.Username);
        user.RoleId.Should().Be(body.Role);
        user.Email.Should().Be(body.Email);
        hasher.VerifyHashedPassword(user, user.Password, body.Password)
              .Should()
              .Be(PasswordVerificationResult.Success);



    }
    [Fact]
    public async Task NewUserIsAbleToLogingAfterRegistration()
    {
        var client = _factory.CreateClient();
        var body = new RegisterManagerRequest
        {
            Email = "test@email",
            Password = "Testpassword1!",
            Role = 1,
            Username = "testusername2"
        };
        var registrationResponse = await client.PostAsJsonAsync("/api/user", body);

        Dictionary<string, string> loginData = new()
        {
            ["Username"] = body.Username,
            ["Password"] = body.Password

        };

        var form = new FormUrlEncodedContent(loginData);

        registrationResponse.Should().Be201Created();
        var loginResponse = await client.PostAsync("/auth", form);
        loginResponse.Should().Be200Ok()
                     .And.HaveHeader("Set-Cookie").And.Match("auth*");

    }

    public Task InitializeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        db.Roles.Add(new RolesEntity
        {
            Id = 1,
            Name = "fake_role"
        });
        db.Users.Add(
                new UserEntity
                {
                    //Id = 1,
                    Email = "fake@email.com",
                    Password = "fakepassword",
                    RoleId = 1,
                    Uname = "fake_user"
                }
                );
        db.SaveChanges();
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        return _factory.ResetDatabaseAsync();
    }
}
