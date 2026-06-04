using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.WebUtilities;
using core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using core.Interfaces;
using core.Domain;

namespace core.Services;



///<inheritdoc />
public class InvitesService : BaseDataService, IInvitesService
{
    private readonly ILogger<InvitesService> _logger;
    public InvitesService(IApplicationDbContext context,
            ILogger<InvitesService> logger) : base(context)
    {
        _logger = logger;
    }

    public async Task<UserDto?> ValidateToken(string token,
                                           CancellationToken ct = default)
    {
        byte[] hashedToken = SHA256.HashData(WebEncoders
                                            .Base64UrlDecode(token));
        UserEntity? issuer = await _context.ValidateInviteTokenStoredProcAsync(hashedToken);
        if (issuer is null)
        {
            _logger.LogInformation($"Request with invalid invite token");
            return null;
        }

        _logger.LogInformation($"Request to validate token created by {issuer?.Uname} : success");
        return issuer!.ToDto();
    }

    public async Task<InviteTokenModel> GenNewInviteAsync(string issuerName)
    {
        byte[] bytes = RandomNumberGenerator.GetBytes(32);
        long issuerId = await GetUserIdByName(issuerName);
        var entity = AddNewInvite(bytes, issuerId);
        _logger.LogInformation($"New invite was added by {issuerName}");
        await SaveChangesAsync();
        return new InviteTokenModel(bytes, entity.Entity.ExpiresAt);
    }

    private EntityEntry<InviteEntity> AddNewInvite(byte[] token, long issuerId)
    {

        byte[] hashedToken = SHA256.HashData(token);
        var now = DateTime.UtcNow;
        var entity = new InviteEntity
        {
            TokenHash = hashedToken,
            ExpiresAt = now.AddMinutes(10),
            CreatedBy = issuerId,

        };
        return _context.Invites.Add(entity);
    }
    private async Task<long> GetUserIdByName(string issuerName) =>
        await _context.Users.AsNoTracking()
                            .Where(u => u.Uname == issuerName && u.RoleId == (long)RoleIds.Manager)
                            .Select(u => u.Id)
                            .SingleAsync();

}

