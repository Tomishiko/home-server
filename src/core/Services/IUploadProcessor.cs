using core.Models;
using core.Models.Generic;

namespace core.Services;

public interface IUploadProcessor{

    Task<Result<string>> ProcessFilePart(FilePartDto filePart);
    Result<string> AddNewFileHandle(FileCreationDto fileDto);

}
