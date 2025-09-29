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
    public static IServiceCollection Startup(this IServiceCollection services, IConfiguration config)
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
        services.AddScoped<IFileService, FileService>();
        services.AddScoped(typeof(IPasswordHasher<>), typeof(PasswordHasher<>));
        services.AddHostedService<BackgroundFileService>();


        JWT jwt = new();
        config.GetSection("JWT").Bind(jwt);
        services.SetAuthentication(jwt);

        return services;
    }
    public static IServiceCollection SetAuthentication(this IServiceCollection services, JWT jwt)
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
                    ValidIssuer = jwt.issuer,
                    ValidAudience = jwt.issuer,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.key))
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
