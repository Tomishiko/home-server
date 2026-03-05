namespace core.Models;

public record InviteTokenModel(byte[] Value, DateTime Expiration);
