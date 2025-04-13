namespace web.Extensions;

using System.Text;
using Data.Shared;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Data.Core;
using Microsoft.AspNetCore.Identity;
using web.Services;
using Microsoft.EntityFrameworkCore;
using core.Services;
using core.Models;


public static class ServiceExtensions
{
    public static IServiceCollection SetServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddOptions<JWT>().BindConfiguration("JWT");
        services.AddSingleton<StreamedFileCompositor>();
        services.AddSingleton<ICoreFS, CoreFS>();
        services.AddSingleton<JWTGen>();
        services.AddTransient<IMpvService, Mpv>();
        services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(config.GetValue<string>("ConnectionString")));
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ILogService, LogService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped(typeof(IPasswordHasher<>),typeof(PasswordHasher<>));
        return services;
    }
    public static IServiceCollection SetAuthentication(this IServiceCollection services, IConfiguration config)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = config.GetValue<string>("JWT:Issuer"),
                    ValidAudience = config.GetValue<string>("JWT:Issuer"),
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.GetValue<string>("JWT:key")))
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = ctx =>
                    {
                        if (string.IsNullOrEmpty(ctx.Token) && ctx.Request.Cookies.TryGetValue("AspNet.Id", out string? cookieToken))
                        {
                            if (!string.IsNullOrEmpty(cookieToken))
                                ctx.Token = cookieToken;
                        }

                        return Task.CompletedTask;
                    }
                };
            });
        return services;
    }
}

public static class ConfigurationExtensions
{
}
