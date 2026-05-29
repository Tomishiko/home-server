using System.Collections.Concurrent;
using core.Domain;
using core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace core.Services;

public class FileStateBackupWorker : BackgroundService
{
    private const int _batchSize = 16;
    private readonly ILogger<FileStateBackupWorker> _logger;
    private readonly ConcurrentDictionary<Guid, IUploadingFileState> _activeSessions;
    private readonly FileUploadStateBackupContext[] _batch;
    private readonly IServiceScopeFactory _factory;


    public FileStateBackupWorker(ILogger<FileStateBackupWorker> logger,
                                 UploadSessionMonitor sessionMonitor,
                                 IServiceScopeFactory factory)
    {
        _logger = logger;
        _activeSessions = sessionMonitor.ActiveSessions;
        _factory = factory;

        _batch = new FileUploadStateBackupContext[_batchSize];

        for (int i = 0; i < _batch.Length; i++)
        {
            _batch[i] = new FileUploadStateBackupContext();
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {

        _logger.LogInformation("Starting background service");
        while (!stoppingToken.IsCancellationRequested)
        {

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            if (_activeSessions.IsEmpty) continue;

            try
            {
                using var scope = _factory.CreateAsyncScope();
                var writeServcice = scope.ServiceProvider.GetRequiredService<IDirectDbQuery>();
                int count = 0;

                foreach (var session in _activeSessions)
                {
                    var fileWriter = session.Value;

                    if (!fileWriter.TryGetSnapshotBackup(out var snapshot))
                    {
                        continue;
                    }

                    _batch[count].Id = snapshot.Id;
                    _batch[count].Bitfield = snapshot.Bitfield;
                    _batch[count].PartsWritten = snapshot.PartsWritten;
                    count++;

                    if (count == _batch.Length)
                    {
                        await FlushBuffer(writeServcice, count, stoppingToken);
                        count = 0;
                    }
                }

                if (count > 0)
                {
                    await FlushBuffer(writeServcice, count, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Shutting down background backups");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical error in backup loop");
            }
        }
    }

    private async Task FlushBuffer(IDirectDbQuery db, int count, CancellationToken ct)
    {
        var bufferSlice = _batch.AsMemory(0, count);

        try
        {
            int updated = await db.UpdateFileUploadState(bufferSlice, ct);
            _logger.LogInformation($"Updated file upload state  records:{updated}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating DB state for batch");
        }

    }

}

