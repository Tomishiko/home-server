using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.WebUtilities;
using core.Models;
using Data.Core;
using Data.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace core.Services;



///<inheritdoc />
public class InvitesService : BaseDataService, IInviteService
{
    private readonly ILogger<InvitesService> _logger;
    public InvitesService(ApplicationDbContext context,
            ILogger<InvitesService> logger) : base(context)
    {
        _logger = logger;
    }

    public async Task<User?> ValidateToken(string token,
                                           CancellationToken ct = default)
    {
        byte[] hashedToken = SHA256.HashData(WebEncoders
                                            .Base64UrlDecode(token));
        var tokenParam = new NpgsqlParameter
        {
            ParameterName = "token",
            NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Bytea,
            Value = hashedToken
        };

        UserEntity? issuer = await _context.Users
            .FromSqlRaw($"SELECT * FROM GetInviteIssuerByToken(@token)", tokenParam)
            .AsNoTracking()
            .SingleOrDefaultAsync(ct);

        if (issuer is null)
        {
            _logger.LogInformation($"Request with invalid invite token");
            return null;
        }

        _logger.LogInformation($"Request to validate token created by {issuer?.Uname} : success");
        return User.FromEntity(issuer!);

    }

    public async Task<byte[]> GenNewInvite(string issuerName, CancellationToken ct = default)
    {
        byte[] bytes = RandomNumberGenerator.GetBytes(32);
        uint issuerId = await GetUserIdByName(issuerName);
        AddNewInvite(bytes, issuerId, ct);
        _logger.LogInformation($"New invite was added by {issuerName}");
        await SaveChangesAsync();
        return bytes;
    }

    private EntityEntry<InviteEntity> AddNewInvite(byte[] token, uint issuerId, CancellationToken ct)
    {

        byte[] hashedToken = SHA256.HashData(token);
        var now = DateTime.UtcNow;
        var entity = new InviteEntity
        {
            TokenHash = hashedToken,
            ExpiresAt = now.AddMinutes(10),
            CreatedBy = issuerId,// TODO dont forget to change

        };
        return _context.Invites.Add(entity);
    }
    private async Task<uint> GetUserIdByName(string issuerName) =>
        await _context.Users.AsNoTracking()
                            .Where(u => u.Uname == issuerName && u.Role!.Name == "manager")
                            .Select(u => u.Id)
                            .SingleAsync();

}
