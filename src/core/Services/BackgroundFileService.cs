namespace core.Services;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using core.Interfaces;
using core.Models;
using Microsoft.Extensions.Options;

public sealed class BackgroundFileService : BackgroundService
{
    private readonly ILogger<BackgroundService> _logger;
    private readonly IServiceProvider _services;
    private readonly FileUploadOptions _fileUploadOptions;

    Timer? _timer;

    public BackgroundFileService(ILogger<BackgroundService> logger, IServiceProvider services, IOptions<FileUploadOptions> config)
    {
        _logger = logger;
        _services = services;
        _fileUploadOptions = config.Value;
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
    private async Task PerformCheckForDatedFiles()
    {
        using var scope = _services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var files = context.Files.Where(f => f.IsDeleted)
                                 .AsAsyncEnumerable();

        await Parallel.ForEachAsync(files, (file, cancelationToken) =>
        {
            string path = Path.Combine(_fileUploadOptions.StoragePath, file.UUID);
            if (!File.Exists(path))
            {
                return ValueTask.CompletedTask;
            }
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
    public override void Dispose()
    {
        _timer?.Dispose();
    }
}
