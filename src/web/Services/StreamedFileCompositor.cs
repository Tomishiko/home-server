using System;
using web.Interfaces;
using web.Models;

namespace web.Services;

public class StreamedFileCompositor
{
    private ILogger<StreamedFileCompositor> _logger;
    public Dictionary<string, IStreamedFile> StreamedFiles;

    public StreamedFileCompositor(ILogger<StreamedFileCompositor> logger)
    {
        _logger = logger;
        StreamedFiles = new Dictionary<string, IStreamedFile>();
    }
    public void CloseEventHandler(object? sender, string id)
    {
        StreamedFiles.Remove(id);
        var file = (IStreamedFile)sender; // TODO move it to eventargs
        _logger.LogInformation($"File {file?.FileName} handle was closed. {file?.FileSize} bytes was written");
    }

}
