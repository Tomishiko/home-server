namespace core.Services;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Data.Core;
using Data.Models;
using Microsoft.Extensions.Configuration;

public sealed class BackgroundFileService : BackgroundService
{
    readonly ILogger<BackgroundService> _logger;
    readonly IServiceProvider _services;
    readonly string _basePath;

    Timer? _timer;

    public BackgroundFileService(ILogger<BackgroundService> logger, IServiceProvider services, IConfiguration config)
    {
        _logger = logger;
        _services = services;
        _basePath = config.GetValue<string>("FilesLocation")
            ?? throw new NullReferenceException("File storage location is not specified");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("BackgroundFileService running...");
        try
        {
            await PerformCheckForDatedFiles();

            using PeriodicTimer timer = new(TimeSpan.FromHours(1));
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await PerformCheckForDatedFiles();
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("BackgroundFileService stopping...");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error encountered");
        }
    }
    public override void Dispose()
    {
        _timer?.Dispose();
    }
    private async Task PerformCheckForDatedFiles()
    {
        using var scope = _services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var files = context.Files.Where(f => f.IsDeleted)
                                 .AsAsyncEnumerable();

        await Parallel.ForEachAsync(files, (file, cancelationToken) =>
        {
            var path = $"{_basePath}/{file.UUID}";
            Debug.Assert(File.Exists(path));
            try
            {
                File.Delete(path);
                context.Files.Remove(file);
                _logger.LogInformation($"File {file.Name}-{file.UUID} was removed by background cleaning");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while deleting file");
            }
            return ValueTask.CompletedTask;
        });
        await context.SaveChangesAsync();

    }
}
