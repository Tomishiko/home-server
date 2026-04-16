using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using web.Models;

namespace Tests.Integration.Infra;

public class SampleMockAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public SampleMockAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim(AppClaimTypes.Name, "MockUser"),
            new Claim(AppClaimTypes.Role, "manager"),
            new Claim(AppClaimTypes.Identity, "1")
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name, AppClaimTypes.Name, AppClaimTypes.Role);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
