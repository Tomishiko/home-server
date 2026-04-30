using System.IO.Pipelines;
using core.Interfaces;
using core.Models;
using core.Models.Generic;

namespace core.Services;

public interface IUploadProcessor
{

    Task<Result<UploadPartSuccess>> ProcessFilePartPipe(Guid uuid, int currentPart, PipeReader pipe, CancellationToken ct);
    Task<Result<FileHandshakeResponseDto>> AddNewFileHandleAsync(
        FileCreationDto fileDto, IApplicationDbContext db, IPhysicalFileWriterFactory physicalFileWriterFactory,
        FileUploadOptions fileUploadOptions);

}
