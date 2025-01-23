using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using mvc_server.Models;

namespace mvc_server.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection SetOptions(this IServiceCollection services)
    {
        services.AddOptions<JWT>().BindConfiguration("JWT");
        return services;
    }
    public static IServiceCollection SetAuthentication(this IServiceCollection services,IConfiguration config)
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
                        Console.WriteLine($"{ctx.Request.Headers}");
                        if (ctx.Request.Headers["X-Requested-With"] != "XMLHttpRequest")
                        {
                            ctx.Request.Cookies.TryGetValue("AspNet.Id", out string token);
                            if (!string.IsNullOrEmpty(token))
                                ctx.Token = token;
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
