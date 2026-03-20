using Microsoft.AspNetCore.Mvc;
using core.Models;
using Microsoft.Extensions.Options;
using core.Services;
namespace web.Helpers;

public class FileUploadHelperService
{
    private readonly FileUploadOptions _options;
    //private readonly ICoreFS _fs;
    public FileUploadHelperService(IOptions<FileUploadOptions> options)
    {
        _options = options.Value;
    }
    public IActionResult ServeFile(ControllerBase controller, FileMeta fileRec)
    {

        if (_options.UseAccelRedirect)// offload file upload to nginx
        {
            controller.Response.Headers.ContentType = "application/octet-stream";
            controller.Response.Headers.ContentDisposition = $"attachment; filename=\"{fileRec.Name}.{fileRec.Ext}\"";
            controller.Response.Headers["X-Accel-Redirect"] = $"{_options.AccelPrefix}{fileRec.UUID}";
            return new EmptyResult();
        }
        else // serve file directly
        {
            return controller.PhysicalFile(Path.Combine($"{_options.StoragePath}", fileRec.UUID),
                                   contentType: "application/octet-stream",
                                   enableRangeProcessing: true,
                                   fileDownloadName: fileRec.Name + fileRec.Ext);
        }

    }
}
