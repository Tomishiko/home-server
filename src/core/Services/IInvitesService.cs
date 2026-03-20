using core.Models;

namespace core.Services;
public interface IInvitesService
{
    public Task ValidateToken(string token, CancellationToken ct = default);
    public Task<InviteTokenModel> GenNewInviteAsync(string issuerName);
}
