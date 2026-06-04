using System.Collections.Concurrent;
using core.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace core.Services;

public class UploadSessionMonitor
{
    private readonly IServiceScopeFactory scopeFactory;
    private readonly ILogger<UploadSessionMonitor> _logger;
    public readonly ConcurrentDictionary<Guid, IUploadingFileState> ActiveSessions;
    public readonly ConcurrentDictionary<string, Guid> UuidByFingerprint;

    public UploadSessionMonitor(ILogger<UploadSessionMonitor> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        ActiveSessions = new ConcurrentDictionary<Guid, IUploadingFileState>();
        UuidByFingerprint =
            new ConcurrentDictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
        this.scopeFactory = scopeFactory;
    }

    public async void OnCloseEventAsync(object? sender, CloseFileEventArgs e)
    {
        using var scope = scopeFactory.CreateScope();
        var logService = scope.ServiceProvider.GetRequiredService<ILogService>();
        var fileService = scope.ServiceProvider.GetRequiredService<IFileService>();
        var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        if (!ActiveSessions.TryRemove(e.FileId, out IUploadingFileState? finishedFile))
        {
            // TODO: handle error of removing
            var log = new LogDto(0, $"Was not able to remove file {e.FileName}:{e.FileId} from streaming queue",
                    e.ClosedAt.ToUniversalTime(), "StreamedFileCompositor");
            await logService.AddNewLog(log);
            return;
        }
        UuidByFingerprint.Remove(finishedFile.FileFingerprint, out _);
        // Get "extension" and file's name if possible
        int extIndex = finishedFile.FileName.LastIndexOf('.');
        string ext, fname;

        if (extIndex != -1)
        {
            ext = finishedFile.FileName[extIndex..];
            fname = finishedFile.FileName[..extIndex];
        }
        else
        {
            ext = string.Empty;
            fname = finishedFile.FileName;
        }

        fileService.StageNewFileRecord(finishedFile.Uuid.ToString(), ext, fname,
                    finishedFile.FileSize, finishedFile.OwnerId, true);

        //TODO: we might be able to optimize this double trip
        await db.FileUploadState.Where(f => f.Id == finishedFile.Uuid).ExecuteDeleteAsync();

        int changes = await db.SaveChangesAsync();
        finishedFile.CloseEvent -= OnCloseEventAsync;
        finishedFile.Dispose();
        _logger.LogInformation($"File {e.FileName}  handle was closed.  {e.FileSize} bytes was written");
    }

}

