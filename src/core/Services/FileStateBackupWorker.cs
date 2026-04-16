using System.Collections.Concurrent;
using core.Domain;
using core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace core.Services;

public class FileStateBackupWorker : BackgroundService
{
    private const int _batchSize = 8;
    private readonly ILogger<FileStateBackupWorker> _logger;
    private readonly ConcurrentDictionary<Guid, IUploadingFileState> _activeSessions;
    private readonly IServiceScopeFactory _factory;
    private readonly FileStateBackupContext[] _sharedBuffer;

    public FileStateBackupWorker(ILogger<FileStateBackupWorker> logger,
                                 UploadSessionMonitor sessionMonitor,
                                 IServiceScopeFactory factory)
    {
        _logger = logger;
        _activeSessions = sessionMonitor.ActiveSessions;
        _factory = factory;
        _sharedBuffer = new FileStateBackupContext[_batchSize];
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {

        _logger.LogInformation("Starting background service");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

                if (_activeSessions.IsEmpty) continue;

                using var scope = _factory.CreateAsyncScope();
                var writeServcice = scope.ServiceProvider.GetRequiredService<IDirectDbQueryService>();

                int bufferPointer = 0;

                foreach (var session in _activeSessions)
                {
                    var fileWriter = session.Value;
                    if (!fileWriter.IsDirty) continue;

                    _sharedBuffer[bufferPointer++] = fileWriter.GetSnapshot();

                    if (bufferPointer == _batchSize)
                    {
                        await FlushBuffer(writeServcice, bufferPointer);
                        bufferPointer = 0;
                    }
                }

                if (bufferPointer > 0)
                {
                    await FlushBuffer(writeServcice, bufferPointer);
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
    private async Task FlushBuffer(IDirectDbQueryService db, int count)
    {
        var bufferSlice = _sharedBuffer.AsSpan(0, count);

        try
        {
            await db.UpdateFileUploadState(bufferSlice);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating DB state for batch");
        }
        finally
        {

            for (int i = 0; i < count; i++)
            {
                _sharedBuffer[i]?.Dispose();
                _sharedBuffer[i] = null!;
            }
        }

    }

}
