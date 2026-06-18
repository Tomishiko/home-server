using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.WebUtilities;
using core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using core.Interfaces;
using core.Domain;
using core.Models.Generic;
using core.DomainExceptions;
using Microsoft.AspNetCore.Mvc;

namespace core.Services;



///<inheritdoc />
public class InvitesService : BaseDataService, IInvitesService
{
    private readonly ILogger<InvitesService> _logger;
    private readonly IUserService _userService;

    public InvitesService(IApplicationDbContext context,
            ILogger<InvitesService> logger,
            IUserService userService) : base(context)
    {
        _logger = logger;
        _userService = userService;
    }

    public async Task<Result<ValidTokenDetail>>
        ValidateToken(string token, CancellationToken ct = default)
    {
        byte[] hashedToken = SHA256.HashData(WebEncoders
                                            .Base64UrlDecode(token));
        ValidTokenDetail? validToken = await _context.ValidTokenDetails
            .FirstOrDefaultAsync(t => t.TokenHash == hashedToken, ct);

        if (validToken is null)
        {
            return new Error("Invite token is invalid or expired");
        }

        return validToken;
    }

    public async Task<InviteTokenModel> GenNewInviteAsync(long issuerId)
    {
        byte[] bytes = RandomNumberGenerator.GetBytes(32);
        var entry = AddNewInvite(bytes, issuerId);
        await SaveChangesAsync();

        _logger.LogInformation("New invite {InviteId} was added by user {UserId}", issuerId, entry.Entity.Id);
        return new InviteTokenModel(bytes, entry.Entity.ExpiresAt);
    }

    public async Task<Result<int>> ConsumeToken(string token,
                                                UserCreationDto newUserDto)
    {

        byte[] hashedToken = SHA256.HashData(WebEncoders
                                            .Base64UrlDecode(token));

        var invite = await _context.Invites.Where(i => i.TokenHash == hashedToken)
                                           .Include(i => i.CreatedBy)
                                           .SingleOrDefaultAsync();
        if (invite is null)
        {
            return new Error("Invalid invite token");
        }

        newUserDto = newUserDto with { CreatedBy = invite.CreatedBy!.Uname };

        var stageResult = await _userService.StageNewUser(newUserDto);

        if (stageResult.IsFailure)
        {
            return stageResult.Error;
        }

        invite.UsedBy = stageResult.Value.Entity;
        invite.UsedAt = DateTime.UtcNow;

        try
        {
            return await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return new Error("A registration conflict occurred. Please try again.");
        }
    }


    private EntityEntry<InviteEntity> AddNewInvite(byte[] token, long issuerId)
    {

        byte[] hashedToken = SHA256.HashData(token);
        var now = DateTime.UtcNow;
        var entity = new InviteEntity
        {
            TokenHash = hashedToken,
            ExpiresAt = now.AddMinutes(10),
            CreatedById = issuerId,

        };
        return _context.Invites.Add(entity);
    }

    private async Task<long> GetUserIdByName(string issuerName) =>
        await _context.Users.AsNoTracking()
                            .Where(u => u.Uname == issuerName && u.RoleId == (long)RoleIds.Manager)
                            .Select(u => u.Id)
                            .SingleAsync();

}

