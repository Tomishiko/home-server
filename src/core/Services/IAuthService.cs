namespace core.Services;
using core.Models;

public interface IAuthService
{

    ///<summary>Checks if the user is in DB</summary>
    ///<returns>True if user is in DB and passwords match, false othervise</returns>
    ///<exception cref="ArgumentNullException">Thrown when <paramref name="user"/> is null</exception>
    ///<exception cref="InvalidOperationException">Thrown when there is more then one corresponding usernames</exception>
    bool Authenticate(User user);
}
