using core.Models;

namespace core.Services;

public interface IInviteService
{

    ///<summary>
    ///Checks if token exist in DB and is not expired or used
    ///</summary>
    ///<returns>Issuer of the token or null when token is not valid</returns>
    public Task<UserDto?> ValidateToken(string token, CancellationToken ct = default);

    public Task<InviteTokenModel> GenNewInviteAsync(string issuerName);
}
