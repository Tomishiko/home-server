namespace web.Models;

public record NewInviteTokenResponse(
            string Token,
            DateTime Expiration
        );
