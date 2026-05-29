using System.Buffers;
using System.Diagnostics;
using System.IO.Pipelines;
using core.Domain;
using core.Interfaces;
using core.Models;
using core.Models.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace core.Services;

public class UploadProcessor : IUploadProcessor
{

    private readonly UploadSessionMonitor _fileCompositor;
    private readonly ILogger<IUploadProcessor> _logger;
    private readonly FileUploadOptions _fileOptions;


    public UploadProcessor(UploadSessionMonitor fileCompositor, ILogger<IUploadProcessor> logger)
    {
        _fileCompositor = fileCompositor;
        _logger = logger;
    }

    /// We pass DI args this way here to avoid  allocations and unimportant
    /// logic on hot path as much as possible
    public async Task<Result<FileHandshakeResponseDto>> AddNewFileHandleAsync(FileCreationDto fileDto,
                                                                              IApplicationDbContext db,
                                                                              IPhysicalFileWriterFactory physicalFileWriterFactory,
                                                                              FileUploadOptions fileUploadOptions)
    {
        var UniqueID = Guid.NewGuid();

        if (!_fileCompositor.UuidByFingerprint.TryAdd(fileDto.Fingerprint, UniqueID))
        {

            var existingId = _fileCompositor.UuidByFingerprint[fileDto.Fingerprint];

            if (!_fileCompositor.ActiveSessions
                    .TryGetValue(existingId, out var entry))
            {
                return new Error("Something unexpected happened", 500);
            }

            return new FileHandshakeResponseDto(entry.Uuid.ToString(),
                                                fileDto.PartSize,
                                                entry.WindowStart,
                                                entry.PartsBitfield);
        }

        //if (await db.FileUploadState
        //        .FirstOrDefaultAsync(x => x.Fingerprint == fileDto.Fingerprint)
        //        is FileUploadStateEntity entry)
        //{
        //    return new FileHandshakeResponseDto(entry.Id.ToString(),
        //                                        fileDto.PartSize,
        //                                        entry.PartsWritten);
        //}

        var streamedFile = new UploadingFileState(fileDto, fileUploadOptions.StoragePath,
                UniqueID, physicalFileWriterFactory);

        streamedFile.CloseEvent += _fileCompositor.OnCloseEventAsync;

        if (!_fileCompositor.ActiveSessions.TryAdd(UniqueID, streamedFile))
        {
            _fileCompositor.UuidByFingerprint.Remove(fileDto.Fingerprint, out _);
            return new Error("Unexpected UUID collision", 500);
        }


        db.FileUploadState.Add(new FileUploadStateEntity
        {
            Id = streamedFile.Uuid,
            Fingerprint = streamedFile.FileFingerprint,
            Metadata = new FileWriterMeta(streamedFile.FileSize,
                                          streamedFile.PartSize,
                                          streamedFile.FileName,
                                          streamedFile.OwnerId,
                                          streamedFile.TotalFileParts),
            PartsBitfield = 0,
            PartsWritten = 0
        });

        _logger.LogInformation(
                $"New file registered for uplaod Filename: {fileDto.FileName}, Filesize:{fileDto.FileSize}");

        await db.SaveChangesAsync();

        string uuid = UniqueID.ToString();
        return new FileHandshakeResponseDto(uuid, fileDto.PartSize, 0, 0);

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
