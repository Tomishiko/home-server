using System.Buffers;
using System.IO.Pipelines;
using core.Domain;
using core.Interfaces;
using core.Models;
using core.Models.Generic;
using Microsoft.AspNetCore.Mvc;
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

    public async Task<Result<FileHandshakeResponseDto>> AddNewFileHandleAsync(FileCreationDto fileDto, IApplicationDbContext db)
    {

        var UniqueID = Guid.NewGuid();
        var streamedFile = new UploadingFileState(fileDto, _fileOptions.StoragePath,
                UniqueID, fileDto.Fingerprint);

        streamedFile.CloseEvent += _fileCompositor.OnCloseEventAsync;

        if (!_fileCompositor.ActiveSessions.TryAdd(UniqueID, streamedFile))
        {
            return new Error("Unexpected UUID collision", 500);
        }


        db.FileUploadState.Add(new FileUploadStateEntity
        {
            Id = streamedFile.Id,
            Fingerprint = streamedFile.FileFingerprint,
            Metadata = new FileWriterMeta(streamedFile.FileSize,
                                          streamedFile.PartSize,
                                          streamedFile.FileName,
                                          streamedFile.OwnerId,
                                          streamedFile.TotalFileParts),
            PartsBitfield = null,
            PartsWritten = 0
        });

        _logger.LogInformation(
                $"New file registered for uplaod Filename: {fileDto.FileName}, Filesize:{fileDto.FileSize}");

        await db.SaveChangesAsync();
        return new FileHandshakeResponseDto(UniqueID.ToString(), fileDto.PartSize);


    }

    public Task<Result<UploadPartSuccess>> ProcessFilePartPipe(Guid uuid,
                                            int currentPart,
                                            PipeReader reader,
                                            CancellationToken ct = default)
    {
        if (!_fileCompositor.ActiveSessions
                .TryGetValue(uuid, out IUploadingFileState? fileWriter))
        {

            return Task.FromResult<Result<UploadPartSuccess>>(
                new Error("No open file handle coresponding to provided key")
            );
        }

        return fileWriter.WritePartFromPipeAsync(currentPart, reader, ct, _logger);


    }
}
