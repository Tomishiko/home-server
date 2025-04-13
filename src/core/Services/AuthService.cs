namespace core.Services;
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;
using core.Models;
using Data.Shared;
using Data.Models;

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
    public bool Authenticate(User user)
    {
        ArgumentNullException.ThrowIfNull(user, nameof(user));
        var hasher = new PasswordHasher<User>();
        var providedPassword = hasher.HashPassword(user, user.Password);
        var hashedPassword = _userRepo.Query()
            .Where(u => u.Uname == user.Uname)
            .Select(u => u.Password)
            .SingleOrDefault();

        return !string.IsNullOrEmpty(hashedPassword) &&
                hasher.VerifyHashedPassword(user, hashedPassword, user.Password) == PasswordVerificationResult.Success;
    }

}
