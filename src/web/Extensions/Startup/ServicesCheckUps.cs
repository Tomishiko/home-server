using Data.Core;
namespace web.Extensions;

public static class ServicesCheckUps
{

    public static async Task<WebApplication> PerformServicesCheckups(this WebApplication app)
    {
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var dbContext = services.GetRequiredService<ApplicationDbContext>();
            if (!await dbContext.Database.CanConnectAsync())
            {
                throw new Exception("Database is unreachable!");
            }
        }

        return app;
    }
}
