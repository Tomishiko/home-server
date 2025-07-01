using web.Interfaces;
using System.Collections.Concurrent;
using core.Services;
using core.Models;
using web.Models;

namespace web.Services;

public class StreamedFileCompositor
{
    private ILogger<StreamedFileCompositor> _logger;
    private readonly IServiceScopeFactory scopeFactory;
    public ConcurrentDictionary<string, IStreamedFile> StreamedFiles;

    public StreamedFileCompositor(ILogger<StreamedFileCompositor> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        StreamedFiles = new ConcurrentDictionary<string, IStreamedFile>();
        this.scopeFactory = scopeFactory;
    }
    public async void CloseEventHandlerAsync(object? sender, CloseFileEventArgs e)
    {
        using var scope = scopeFactory.CreateScope();
        var logService = scope.ServiceProvider.GetRequiredService<ILogService>();
        var fileService = scope.ServiceProvider.GetRequiredService<IFileService>();
        IStreamedFile? finishedFile;

        if (!StreamedFiles.TryRemove(e.FileId, out finishedFile))
        {
            // TODO: handle error of removing
            Log log = new Log(
                    $"Was not able to remove file {e.FileName}:{e.FileId} from streaming queue",
                    e.ClosedAt.ToUniversalTime(), "StreamedFileCompositor");
            await logService.NewLogAsync(log);
            await logService.SaveChangesAsync();
            return;
        }

        // Get "extension" and file's name if possible
        int extIndex = finishedFile.FileName.LastIndexOf('.');
        string ext, fname;

        if (extIndex != -1)
        {
            ext = finishedFile.FileName.Substring(extIndex);
            fname = finishedFile.FileName.Substring(0, extIndex);
        }
        else
        {
            ext = string.Empty;
            fname = finishedFile.FileName;
        }

        await fileService.NewFileRecordAsync(finishedFile.Id, ext, fname,
                    finishedFile.FileSize, finishedFile.OwnerId, true);

        int changes = await fileService.SaveChangesAsync();
        _logger.LogInformation($"File {e.FileName}  handle was closed.  {e.FileSize} bytes was written");
    }

}
