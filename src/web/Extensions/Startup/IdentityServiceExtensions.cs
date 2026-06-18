using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using core.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using web.Helpers;
using web.Models;

namespace web.Extensions;

public static class IdentityServiceExtensions
{

    public static IServiceCollection SetAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication("Cookies")
        .AddCookie(options =>
        {
            options.Cookie.Name = "auth";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
            options.LoginPath = "/login";
            options.Events.OnRedirectToLogin = context =>
            {
                var request = context.Request;
                var response = context.Response;
                if (!request.IsJsonRequest())
                {
                    response.Redirect(context.RedirectUri);

                }
                else
                {
                    response.StatusCode = StatusCodes.Status401Unauthorized;
                    response.Headers["X-Target-Url"] = context.RedirectUri;

                }
                return Task.CompletedTask;
            };
        });
        //JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

        services.AddAuthorizationBuilder()
            .SetDefaultPolicy(new AuthorizationPolicyBuilder()
                                .RequireAuthenticatedUser()
                                .RequireClaim(AppClaimTypes.Name)
                                .RequireClaim(AppClaimTypes.Identity)
                                .RequireClaim(AppClaimTypes.Role)
                                .Build()
                              )
            .AddPolicy("ManagerOnly", policy => policy.RequireAuthenticatedUser()
                                                      .RequireRole(nameof(RoleIds.Manager))
                                                      .RequireClaim(AppClaimTypes.Name)
                                                      .RequireClaim(AppClaimTypes.Identity));
        return services;
    }
}
