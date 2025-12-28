using System.Security.Cryptography;
using core.Models;
using Data.Core;
using Data.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace core.Services;



public class InvitesService : BaseDataService, IInviteService
{
    public InvitesService(ApplicationDbContext context) : base(context) { }

    ///<inheritdoc />
    public async Task<User?> ValidateToken(byte[] token, CancellationToken ct = default)
    {
        byte[] hashedToken = SHA256.HashData(token);
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

        return issuer is null ? null : User.FromEntity(issuer);
        //return _context.Invites
        //                .AnyAsync(i => i.TokenHash == hashedToken
        //                            && i.UsedAt == null
        //                            && i.ExpiresAt > now);
    }

    public async Task<byte[]> GenNewInvite(string issuerName, CancellationToken ct = default)
    {
        byte[] bytes = RandomNumberGenerator.GetBytes(32);
        uint issuerId = await GetUserIdByName(issuerName);
        await AddNewInvite(bytes, issuerId, ct);
        return bytes;
    }

    private Task<int> AddNewInvite(byte[] token, uint issuerId, CancellationToken ct)
    {

        byte[] hashedToken = SHA256.HashData(token);
        var now = DateTime.UtcNow;
        var entity = new InviteEntity
        {
            TokenHash = hashedToken,
            ExpiresAt = now.AddMinutes(10),
            CreatedBy = issuerId,// TODO dont forget to change

        };
        _context.Invites.Add(entity);
        return _context.SaveChangesAsync(ct);
    }
    private async Task<uint> GetUserIdByName(string issuerName) =>
        await _context.Users.AsNoTracking()
                            .Where(u => u.Uname == issuerName && u.Role!.Name == "manager")
                            .Select(u => u.Id)
                            .SingleAsync();

}
