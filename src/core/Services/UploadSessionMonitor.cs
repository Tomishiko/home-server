using System.Collections.Concurrent;
using core.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using core.Interfaces;

namespace core.Services;

public class UploadSessionMonitor
{
    private readonly IServiceScopeFactory scopeFactory;
    private readonly ILogger<UploadSessionMonitor> _logger;
    public ConcurrentDictionary<Guid, IPhysicalFileWriter> ActiveSessions;

    public UploadSessionMonitor(ILogger<UploadSessionMonitor> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        ActiveSessions = new ConcurrentDictionary<Guid, IPhysicalFileWriter>();
        this.scopeFactory = scopeFactory;
    }

    public async void OnCloseEventAsync(object? sender, CloseFileEventArgs e)
    {
        using var scope = scopeFactory.CreateScope();
        var logService = scope.ServiceProvider.GetRequiredService<ILogService>();
        var fileService = scope.ServiceProvider.GetRequiredService<IFileService>();

        if (!ActiveSessions.TryRemove(e.FileId, out IPhysicalFileWriter? finishedFile))
        {
            // TODO: handle error of removing
            var log = new LogDto(0, $"Was not able to remove file {e.FileName}:{e.FileId} from streaming queue",
                    e.ClosedAt.ToUniversalTime(), "StreamedFileCompositor");
            await logService.AddNewLog(log);
            await logService.SaveChangesAsync();
            return;
        }

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

        await fileService.StageNewFileRecord(finishedFile.Id.ToString(), ext, fname,
                    finishedFile.FileSize, finishedFile.OwnerId, true);

        int changes = await fileService.SaveChangesAsync();
        finishedFile.CloseEvent -= OnCloseEventAsync;
        finishedFile.Dispose();
        _logger.LogInformation($"File {e.FileName}  handle was closed.  {e.FileSize} bytes was written");
    }

}

