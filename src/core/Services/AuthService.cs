namespace core.Services;

using Microsoft.AspNetCore.Identity;
using core.Interfaces;
using core.Models;
using core.Domain;
using core.Models.Generic;
using Microsoft.EntityFrameworkCore;

public class AuthService : BaseDataService, IAuthService
{

    public AuthService(IApplicationDbContext context) : base(context) { }

    ///<summary>Checks if the user is in DB</summary>
    ///<returns>True if user is in DB and passwords match, false othervise</returns>
    ///<exception cref="ArgumentNullException">Thrown when <paramref name="user"/> is null</exception>
    ///<exception cref="InvalidOperationException">Thrown when there is more then one corresponding usernames</exception>
    public async Task<Result<UserDto>> AuthenticateAsync(UserAuthDto user)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(user.Username, nameof(user.Username));
        ArgumentException.ThrowIfNullOrWhiteSpace(user.Password, nameof(user.Password));

        var hasher = new PasswordHasher<UserDto>();
        // TODO:change logic to use usermanager service
        UserEntity? userDB = await _context.Users
                            .Include("Role")
                            .Where(u => u.Uname == user.Username)
                            .SingleOrDefaultAsync();
        if (userDB is null)
        {
            return new Error("Bad credentials");
        }


        UserDto userInfo = userDB.ToDto();

        var verificationResult = hasher.VerifyHashedPassword(userInfo, userDB.Password, user.Password);

        return verificationResult switch
        {
            PasswordVerificationResult.Success => new Success<UserDto>(userInfo),
            PasswordVerificationResult.Failed => new Error("Bad credentials"),
            _ => new Error("Something unexpected happened")

        };

    }

}
