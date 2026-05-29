namespace core.Services;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using core.Interfaces;
using core.Models;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

public sealed class BackgroundFileService : BackgroundService
{
    private readonly ILogger<BackgroundService> _logger;
    private readonly IServiceProvider _services;
    private readonly FileUploadOptions _fileUploadOptions;


    public BackgroundFileService(ILogger<BackgroundService> logger,
                                 IServiceProvider services,
                                 IOptions<FileUploadOptions> config)
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
            await PerformCheckForDatedFiles(stoppingToken);

            using PeriodicTimer timer = new(TimeSpan.FromHours(1));

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await PerformCheckForDatedFiles(stoppingToken);
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
    private async Task PerformCheckForDatedFiles(CancellationToken ct = default)
    {
        using var scope = _services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var filesToDelete = await context.Files.Where(f => f.IsDeleted)
                                               .AsNoTracking()
                                               .ToArrayAsync();
        if (filesToDelete.Length == 0)
        {
            return;
        }

        var deletedIds = new ConcurrentBag<long>();
        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = 8,
            CancellationToken = ct
        };

        await Parallel.ForEachAsync(filesToDelete, parallelOptions, (file, cancelationToken) =>
        {
            string path = Path.Combine(_fileUploadOptions.StoragePath, file.UUID);
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                    _logger.LogInformation("File {FileName}-{FileUUID} was removed by background cleaning",
                    file.Name, file.UUID);
                }
                deletedIds.Add(file.Id);
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "IO error while deleting file {FileUUID}", file.UUID);
            }
            return ValueTask.CompletedTask;
        });

        if (deletedIds.Any())
        {
            await context.Files
            .Where(f => deletedIds.Contains(f.Id))
            .ExecuteDeleteAsync(ct);
        }
    }
}
