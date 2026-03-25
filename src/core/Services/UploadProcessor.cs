using System.Buffers;
using System.IO.Pipelines;
using core.Interfaces;
using core.Models;
using core.Models.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace core.Services;

public class UploadProcessor : IUploadProcessor
{

    private readonly UploadSessionMonitor _fileCompositor;
    private readonly ILogger<IUploadProcessor> _logger;
    private readonly FileUploadOptions _fileOptions;


    public UploadProcessor(UploadSessionMonitor fileCompositor,
                                 ILogger<IUploadProcessor> logger,
                                 IOptions<FileUploadOptions> fileOptions)
    {
        _fileCompositor = fileCompositor;
        _logger = logger;
        _fileOptions = fileOptions.Value;
    }
    public Task<Result<UploadPartSuccess>> ProcessFilePart(FilePartDto filePart)
    {
        if (!_fileCompositor.ActiveSessions
                .TryGetValue(filePart.Id, out IPhysicalFileWriter? fileWriter))
        {

            return Task.FromResult<Result<UploadPartSuccess>>(
                new Error("No open file handle coresponding to provided key")
            );
        }
        return fileWriter.WritePartAsync(filePart.Data,
                                        filePart.BytesRead,
                                        filePart.CurrentPart,
                                        _logger);
    }

    public Result<string> AddNewFileHandle(FileCreationDto fileDto)
    {

        var UniqueID = Guid.NewGuid();
        var streamedFile = new PhysicalFileWriter(fileDto,
                                            _fileOptions.StoragePath,
                                            UniqueID);

        streamedFile.CloseEvent += _fileCompositor.OnCloseEventAsync;
        if (!_fileCompositor.ActiveSessions.TryAdd(UniqueID, streamedFile))
        {
            return new Error("Unexpected UUID collision", 500);
        }
        _logger.LogInformation($"FileHandle opened(OK) Filename: {fileDto.FileName}, Filesize:{fileDto.FileSize}");

        return UniqueID.ToString();


    }

    public Task<Result<UploadPartSuccess>> ProcessFilePartPipe(Guid uuid,
                                            int currentPart,
                                            PipeReader reader,
                                            CancellationToken ct = default)
    {
        if (!_fileCompositor.ActiveSessions
                .TryGetValue(uuid, out IPhysicalFileWriter? fileWriter))
        {

            return Task.FromResult<Result<UploadPartSuccess>>(
                new Error("No open file handle coresponding to provided key")
            );
        }

        return fileWriter.WritePartFromPipeAsync(currentPart, reader, ct, _logger);


    }
}
