using Microsoft.AspNetCore.Mvc;
using core.Models;
using Microsoft.Extensions.Options;
using core.Services;
namespace web.Helpers;

public class FileUploadHelperService
{
    private readonly IOptions<FileUploadOptions> _options;
    private readonly ICoreFS _fs;
    public FileUploadHelperService(IOptions<FileUploadOptions> options, ICoreFS fs)
    {
        _options = options;
        _fs = fs;
    }
    public IActionResult ServeFile(ControllerBase controller, FileMeta fileRec)
    {
        var cfg = _options.Value;
        if (cfg.UseAccelRedirect)// offload file upload to nginx
        {
            controller.Response.Headers.ContentType = "application/octet-stream";
            controller.Response.Headers.ContentDisposition = $"attachment; filename=\"{fileRec.Name}.{fileRec.Ext}\"";
            controller.Response.Headers["X-Accel-Redirect"] = $"{cfg.AccelPrefix}{fileRec.UUID}";
            return new EmptyResult();
        }
        else // serve file directly
        {
            FileStream stream = _fs.GetFileStream(fileRec.UUID);
            return controller.File(stream,
                                   contentType: "application/octet-stream",
                                   enableRangeProcessing: true,
                                   fileDownloadName: fileRec.Name + fileRec.Ext);
        }

    }
}
