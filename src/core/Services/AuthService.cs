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
    public async  Task<bool> AuthenticateAsync(string username)
    {
        if (username.IsNullOrEmpty())
         throw new ArgumentException("Username is needed to authenticate user", nameof(user.Uname));

        var hasher = new PasswordHasher<User>();
        var providedPassword = hasher.HashPassword(user, user.Password);
        try
        {
            var userDB = await _userRepo.Query()
                .Include("Role")
                .Where(u => u.Uname == username)
                .Select(u => new { Role = u.Role, Password = u.Password })
                .SingleAsync();

            Debug.Assert(userDB.Role != null, "User without role in DB");
            user = user with
            {
                Role = userDB.Role.Name
            };

            switch (hasher.VerifyHashedPassword(user, userDB.Password, user.Password))
            {
                case PasswordVerificationResult.Success: return true;
                case PasswordVerificationResult.Failed: return false;
                case PasswordVerificationResult.SuccessRehashNeeded: throw new NotImplementedException(); // TODO: Add rehashing method
                default: return false;

            }
        }
        catch (InvalidOperationException ex)
        {
            return false;
        }

    }

}
