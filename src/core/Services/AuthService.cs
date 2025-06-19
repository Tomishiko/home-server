namespace core.Services;

using Microsoft.AspNetCore.Identity;
using System.Diagnostics;
using core.utils.extensions;
using core.Models;
using Data.Shared;
using Data.Models;
using Microsoft.EntityFrameworkCore;

public class AuthService : IAuthService
{
    IRepository<UserEntity> _userRepo;

    public AuthService(IRepository<UserEntity> userRepo)
    {
        _userRepo = userRepo;
    }
    ///<summary>Checks if the user is in DB</summary>
    ///<returns>True if user is in DB and passwords match, false othervise</returns>
    ///<exception cref="ArgumentNullException">Thrown when <paramref name="user"/> is null</exception>
    ///<exception cref="InvalidOperationException">Thrown when there is more then one corresponding usernames</exception>
    public async Task<AuthResult> AuthenticateAsync(User user)
    {
        if (user.Uname.IsNullOrEmpty())
            throw new ArgumentException("Username is needed to authenticate user", nameof(user.Uname));

        var hasher = new PasswordHasher<User>();
        var providedPassword = hasher.HashPassword(user, user.Password);
        // TODO:change logic to use usermanager service
        try
        {
            var userDB = await _userRepo.Query()
                .Include("Role")
                .Where(u => u.Uname == user.Uname)
                .Select(u => new { Role = u.Role, Password = u.Password, Id = u.Id })
                .SingleAsync();

            Debug.Assert(userDB.Role != null, "User without role in DB");
            user = user with
            {
                Role = userDB.Role.Name,
                Id = userDB.Id
            };

            switch (hasher.VerifyHashedPassword(user, userDB.Password, user.Password))
            {
                case PasswordVerificationResult.Success: return new AuthResult(true, user);
                case PasswordVerificationResult.Failed: return new AuthResult(false, user);
                case PasswordVerificationResult.SuccessRehashNeeded: throw new NotImplementedException(); // TODO: Add rehashing method
                default: return new AuthResult(false, null);

            }
        }
        catch (InvalidOperationException ex)
        {
            return new AuthResult(false, null);
        }

    }

}
