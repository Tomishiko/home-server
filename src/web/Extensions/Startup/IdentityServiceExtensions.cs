using System.Security.Claims;
using core.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using web.Helpers;

namespace web.Extensions;

public static class IdentityServiceExtensions
{

    public static IServiceCollection SetAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options =>
        {
            options.Cookie.Name = "AspNet.Id";
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

        services.AddAuthorization(options =>
        {
            options.DefaultPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .RequireClaim(ClaimTypes.Name)
                .RequireClaim("Id")
                .RequireClaim(ClaimTypes.Role)
                .Build();
            options.AddPolicy("ManagerOnly",
                              policy => policy.RequireClaim(ClaimTypes.Name)
                                              .RequireRole("manager"));
        });
        return services;
    }
}
