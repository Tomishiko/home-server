using web.Interfaces;
using System.Collections.Concurrent;


namespace web.Services;

public class StreamedFileCompositor
{
    private ILogger<StreamedFileCompositor> _logger;
    public ConcurrentDictionary<string, IStreamedFile> StreamedFiles;

    public StreamedFileCompositor(ILogger<StreamedFileCompositor> logger)
    {
        _logger = logger;
        StreamedFiles = new ConcurrentDictionary<string, IStreamedFile>();
    }
    public void CloseEventHandler(object? sender, string id)
    {
        if(StreamedFiles.TryRemove(id,out _)){
            // TODO: handle error of removing
        }
        var file = (IStreamedFile?)sender; // TODO move it to eventargs
        _logger.LogInformation($"File {file?.FileName} handle was closed. {file?.FileSize} bytes was written");
    }

}
