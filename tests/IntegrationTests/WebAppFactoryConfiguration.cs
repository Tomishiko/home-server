using System.Text;
using System.Text.Json;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using web;
using Tests.Integration.Infra;
using SQL;
using Respawn;
using Npgsql;
using System.Data;
using Respawn.Graph;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;


namespace Tests.Infra;

public sealed class WebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string DbConnectionString = "Host=localhost;Username=postgres;Database=testDB;Port=5432";
    private Respawner _respawner = default!;
    private NpgsqlConnection _dbConnection = default!;

    public const string MockScheme = nameof(MockScheme);

    public async Task InitializeAsync()
    {
        DatabaseMigrator.Upgrade(DbConnectionString);
        _dbConnection = new NpgsqlConnection(DbConnectionString);
        await _dbConnection.OpenAsync();
        _respawner = await Respawner.CreateAsync(_dbConnection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"],
            TablesToIgnore = ["schemaversions"]
        });

    }

    public async Task ResetDatabaseAsync()
    {
        // Ensure connection is open before resetting
        if (_dbConnection.State != ConnectionState.Open)
            await _dbConnection.OpenAsync();

        await _respawner.ResetAsync(_dbConnection);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.AddSingleton<IAntiforgery, FakeAntiforgery>();
        });
        var testConfiguration = new Dictionary<string, string?>
        {
            ["DbProvider"] = "postgres",
            ["ConnectionString"] = DbConnectionString,
            ["FilesLocation"] = "/test-files",
            ["FileUpload:PartSize"] = (8 * 1024 * 1024).ToString(),
            ["JWT:Key"] = "uKI5pLFkkFrKa1DaIyNe1wL3K3vadfa36IKlQYYI8iE=",
            ["JWT:Issuer"] = "test-server",
            ["JWT:Expiration"] = "60",
            ["FileServingMode:UseAccelRedirect"] = "false",
            ["Logging:LogLevel:Default"] = "Warning"
        };

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(testConfiguration)
            .Build();
        builder.UseConfiguration(config);

        builder.ConfigureServices(services =>
        {
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    // This will return the ACTUAL internal error message
                    return new BadRequestObjectResult(context.ModelState);
                };
            });
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = MockScheme;
                options.DefaultChallengeScheme = MockScheme;
            }).AddScheme<AuthenticationSchemeOptions, SampleMockAuthenticationHandler>(MockScheme, null);

            services.AddAuthorization(options =>
            {
                var defaultAuthorizationPolicyBuilder = new AuthorizationPolicyBuilder(MockScheme);

                defaultAuthorizationPolicyBuilder =
                    defaultAuthorizationPolicyBuilder.RequireAuthenticatedUser();

                options.DefaultPolicy = defaultAuthorizationPolicyBuilder.Build();
            });
        });
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        if (_dbConnection != null)
        {
            await _dbConnection.CloseAsync();
            await _dbConnection.DisposeAsync();
        }
    }

}
internal class FakeAntiforgery : IAntiforgery
{
    public AntiforgeryTokenSet GetAndStoreTokens(HttpContext httpContext) => new(null, null, null, null);
    public AntiforgeryTokenSet GetTokens(HttpContext httpContext) => new(null, null, null, null);
    public Task<bool> IsRequestValidAsync(HttpContext httpContext) => Task.FromResult(true);
    public Task ValidateRequestAsync(HttpContext httpContext) => Task.CompletedTask;
    public void SetCookieTokenAndHeader(HttpContext httpContext) { }
}
