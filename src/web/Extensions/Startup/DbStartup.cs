using core.Interfaces;
using Data.Core;
using Microsoft.EntityFrameworkCore;

namespace web.Extensions;

public static class DbStartupExtensions
{


    public static IServiceCollection AddDbSupport(this IServiceCollection services, IConfiguration config)
    {
        string provider = config.GetValue<string>("DbProvider") ??
            throw new Exception("Db provider was not specified");
        string connectionString = config.GetConnectionString("DefaultDb") ??
            throw new Exception("No DB connection string was provided");

        switch (provider)
        {
            case "postgres":
                services.AddNpgsqlDataSource(connectionString);
                services.AddDbContext<IApplicationDbContext, ApplicationDbContext>(options =>
                {
                    options.UseNpgsql(connectionString);
                    //.LogTo(Console.WriteLine,
                    //       LogLevel.Information).EnableSensitiveDataLogging();
                });
                break;

        }
        return services;
    }
}
