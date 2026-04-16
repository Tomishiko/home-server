using System.Net;
using System.Net.Http.Json;
using Tests.Infra;
using web.Models;
using FluentAssertions.Web;
using FluentAssertions;
using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Localization;
using core.Models;

namespace Tests.Integration;


public sealed class FileUploadTest(WebAppFactory factory) : IClassFixture<WebAppFactory>, IAsyncLifetime
{
    private readonly WebAppFactory _factory = factory;

    [Fact]
    public async Task HandshakeRegistersNewEntryForFileUpload()
    {
        var client = _factory.CreateClient();
        var fingerprintStub = new byte[32];
        Random.Shared.NextBytes(fingerprintStub);
        var requestBody = new FileHandshake
        {
            FileName = "test.txt",
            FileSize = 1024,
            FileFingerprint = fingerprintStub
        };

        string uuidRegex = @"^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$";


        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        var response = await client.PostAsJsonAsync("/api/upload/handshake", requestBody, options);

        response.Should().Be201Created()
                .And.HaveHeader("Content-Type")
                .And.Match("application/json*");


        var actual = await response.Content.ReadFromJsonAsync<FileHandshakeResponseDto>();
        actual.Should().NotBeNull();
        actual.Uuid.Should().MatchRegex(uuidRegex);
        actual.PartSize.Should().BeGreaterThan(0);


    }
    [Fact]
    public async Task FileUploadInfoIsSavedToDbAfterTimeout()
    {

    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        return _factory.ResetDatabaseAsync();
    }
}
