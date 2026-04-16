using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace IntegrationTests;

public class FileUploadIntegrationTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    public FileUploadIntegrationTest(WebApplicationFactory<Program> factory)
    {
        var myConfiguration = new Dictionary<string, string>
        {

            {"DbProvider", "postgres"},
            {"ConnectionString", "Host=localhost;Username=postgres;Database=myDB;Port=5432"},
            {"FilesLocation", "./files"},

            {"FileServingMode:UseAccelRedirect", "false"},
            {"FileServingMode:AccelPrefix", ""},
            {"FileServingMode:StoragePath", "files"}
        };
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseContentRoot(Path.Combine(Directory.GetCurrentDirectory(), "../../src/web"));
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(myConfiguration);
            });
        });
    }
    [Fact]
    public async Task Test1()
    {
        var client = _factory.CreateClient();
        var responseMessage = await client.GetAsync("/api/files");
        var data = await responseMessage.Content.ReadAsStringAsync();
        Console.WriteLine(data);
    }
}
