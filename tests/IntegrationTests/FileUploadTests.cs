using System.Net.Http.Json;
using web.Models;
using FluentAssertions;
using System.Text.Json;
using core.Models;
using Microsoft.Extensions.DependencyInjection;
using core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using Shared.Helpers;
using System.Net.Mime;
using Xunit.Abstractions;
using Tests.Integration.Infra;

namespace Tests.Integration;


[Collection("Database collection")]
public sealed class FileUploadTest : IClassFixture<WebAppFactory>, IAsyncLifetime
{
    private readonly WebAppFactory _factory;
    private readonly ITestOutputHelper _output;

    public FileUploadTest(WebAppFactory factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
        _factory.Output = output;
    }

    private FileHandshake TestHandshakeBody(int fsize = 1024)
    {
        var fingerprintStub = Generators.RandomString32();

        return new FileHandshake
        {
            FileName = "test.txt",
            FileSize = fsize,
            FileFingerprint = fingerprintStub
        };

    }

    [Fact]
    public async Task HandshakeShouldReturn201WithMetadataForFileUpload()
    {
        var client = _factory.CreateClient();

        var requestBody = TestHandshakeBody();

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
    public async Task FileUploadInfoIsSavedToDbAfterHandshake()
    {
        var client = _factory.CreateClient();
        FileHandshake requestBody = TestHandshakeBody();

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        var response = await client.PostAsJsonAsync("/api/upload/handshake", requestBody, options);

        response.Should().Be201Created();
        var content = await response.Content.ReadFromJsonAsync<FileHandshakeResponseDto>();
        content.Should().NotBeNull();

        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        var dbRecord = await db.FileUploadState.Where(x => x.Id == new Guid(content.Uuid))
                                                                 .SingleOrDefaultAsync();
        dbRecord.Should().NotBeNull();

        Assert.Equal(requestBody.FileName, dbRecord.Metadata.FileName);
        Assert.Equal(requestBody.FileSize, dbRecord.Metadata.FileSize);
        Assert.Equal(requestBody.FileFingerprint, dbRecord.Fingerprint);


    }

    [Fact(DisplayName = "BackupTest")]
    public async Task UploadedPartsIndexesAreBackedupToDbAfterTimeout()
    {
        var client = _factory.CreateClient();

        const int partSize = 1024;
        var handshake = TestHandshakeBody(partSize * 4);
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        var response = await client.PostAsJsonAsync("/api/upload/handshake", handshake, options);
        response.Should().Be201Created();
        var responseContent = await response.Content.ReadFromJsonAsync<FileHandshakeResponseDto>();
        responseContent.Should().NotBeNull();

        var path = $"/api/upload/part/{responseContent.Uuid}";

        // first part
        int partIndex = 1;
        var body = new byte[partSize];
        Random.Shared.NextBytes(body);
        var content = new ByteArrayContent(body);
        content.Headers.ContentType = new MediaTypeHeaderValue(MediaTypeNames.Application.Octet);
        content.Headers.Add("X-Part", partIndex.ToString());
        response = await client.PostAsync(path, content);

        response.Should().Be200Ok();
        var partResponse = await response.Content.ReadFromJsonAsync<UploadPartSuccess>();
        partResponse.Should().NotBeNull();

        Assert.Equal(partIndex, partResponse.PartIndex);
        Assert.Equal(partSize, partResponse.BytesWritten);


        //Second part
        partIndex = 5;
        content.Headers.Remove("X-Part");
        content.Headers.Add("X-Part", partIndex.ToString());
        response = await client.PostAsync(path, content);

        response.Should().Be200Ok();
        partResponse = await response.Content.ReadFromJsonAsync<UploadPartSuccess>();
        partResponse.Should().NotBeNull();

        Assert.Equal(partIndex, partResponse.PartIndex);
        Assert.Equal(partSize, partResponse.BytesWritten);
        await Task.Delay(10000); //Wait for background worker to kick in

        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        var dbRecord = await db.FileUploadState.Where(x => x.Id == new Guid(responseContent.Uuid))
            .SingleOrDefaultAsync();

        dbRecord.Should().NotBeNull();

        byte mask = 0b0010_0010;

        Assert.Equal(mask, dbRecord.PartsBitfield & mask);

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
