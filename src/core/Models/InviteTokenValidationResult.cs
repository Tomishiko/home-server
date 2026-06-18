using core.Domain;

namespace core.Models;

public record InviteTokenValidationResult(UserDto Issuer, InviteEntity? Invite);

