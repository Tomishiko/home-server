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
using Moq;
using core.Interfaces;
using FluentAssertions;
using Data.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;
using web.Models;
using Microsoft.Extensions.Options;


namespace Tests.Integration.Infra;

public sealed class WebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public ITestOutputHelper Output { get; set; }

    private const string DbConnectionString = "Host=127.0.0.1;Username=postgres;Port=5432;Database=testDB";
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
            TablesToIgnore = ["schemaversions"],
            WithReseed = true
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


            // Physical file writer mock
            var writerMock = new Mock<IPhysicalFileWriter>();
            writerMock
                .Setup(x => x.Write(It.IsAny<ReadOnlyMemory<byte>>(), It.IsAny<long>(), CancellationToken.None))
                .Returns(ValueTask.CompletedTask);
            var factoryMock = new Mock<IPhysicalFileWriterFactory>();
            factoryMock
                .Setup(x => x.Create(It.IsAny<string>(), It.IsAny<long>()))
                .Returns(writerMock.Object);
            services.AddTransient(_ => factoryMock.Object);
            services.AddDbContext<IApplicationDbContext, ApplicationDbContext>(options =>
            {
                //Uncomment for more logs from DB

                //options.UseNpgsql(DbConnectionString)
                //       .LogTo(message =>
                //       {
                //           try
                //           {

                //               Output.WriteLine(message);
                //           }
                //           catch (InvalidOperationException ex)
                //           {
                //               // The test has already finished, but the background service is still logging.
                //               // We swallow this to prevent the background service from crashing the test process.
                //           }

                //       }, LogLevel.Information)
                //       .EnableSensitiveDataLogging();
            });

            //Fake Antiforgery
            services.AddSingleton<IAntiforgery, FakeAntiforgery>();
        });

        //Fake Config file
        var testConfiguration = new Dictionary<string, string?>
        {
            ["DbProvider"] = "postgres",
            ["ConnectionStrings:DefaultDb"] = DbConnectionString,
            ["FilesLocation"] = "/test-files",
            ["FileUpload:PartSize"] = "1024",
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

        //Fake auth services
        builder.ConfigureServices(services =>
        {
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    return new BadRequestObjectResult(context.ModelState);
                };
            });
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = MockScheme;
                options.DefaultChallengeScheme = MockScheme;
            }).AddScheme<AuthenticationSchemeOptions, MockAuthenticationHandler>(MockScheme, null);

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
internal class TestFileUploadOptions : FileUploadOptionsClient
{
    // We leave this empty; it inherits properties but NOT attributes
}
