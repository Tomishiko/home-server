using System.IO.Pipelines;
using core.Models;
using core.Models.Generic;

namespace core.Services;

public interface IUploadProcessor
{

    Task<Result<UploadPartSuccess>> ProcessFilePart(FilePartDto filePart);
    Result<string> AddNewFileHandle(FileCreationDto fileDto);
    Task<Result<UploadPartSuccess>> ProcessFilePartPipe(Guid uuid, int currentPart, PipeReader pipe, CancellationToken ct);

}
