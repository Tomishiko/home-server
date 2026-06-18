using core.Domain;
using core.Models;
using core.Models.Generic;

namespace core.Services;

public interface IInvitesService
{

    ///<summary>
    ///Checks if token exist in DB and is not expired or used
    ///</summary>
    ///<returns>Issuer of the token or null when token is not valid</returns>

    Task<Result<ValidTokenDetail>> ValidateToken(string token, CancellationToken ct = default);
    Task<InviteTokenModel> GenNewInviteAsync(long issuerId);
    Task<Result<int>> ConsumeToken(string token, UserCreationDto newUserDto);
}
