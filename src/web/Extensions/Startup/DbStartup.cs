using core.Domain;
using core.Interfaces;
using core.Models;
using Data.Core;
using Data.Infra;
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
                services.AddNpgsqlDataSource(connectionString, builder =>
                {
                    builder.MapComposite<UserCreationDto>("users");
                });
                services.AddDbContext<IApplicationDbContext, PostgresDbContext>(options =>
                {
                    options.UseNpgsql();
                    //.LogTo(Console.WriteLine,
                    //       LogLevel.Information).EnableSensitiveDataLogging();
                });
                break;

        }
        return services;
    }
}
