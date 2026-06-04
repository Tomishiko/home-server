using core.Interfaces;
using core.Models;
using core.Services;
using Data.Core;
using Data.Infra;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols;
using web.Helpers;
using web.Models;

namespace web.Extensions;

public static class AppServicesExtensions
{

    public static IServiceCollection RegisterCoreServices(this IServiceCollection services, IConfiguration config)
    {


        services.AddDbSupport(config);
        services.AddEndpointsApiExplorer();
        services.Configure<FileUploadOptions>(config.GetSection(FileUploadOptions.SectionName));
        services.AddOptions<FileUploadOptionsClient>()
                .Bind(config.GetSection(FileUploadOptionsClient.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();

        services.AddSingleton<UploadSessionMonitor>();
        services.AddSingleton<ICoreFS, CoreFS>();
        services.AddTransient<FileUploadHelperService>();
        services.AddTransient<IPhysicalFileWriterFactory, PhysicalFileWriterFactory>();
        services.AddScoped<IInvitesService, InvitesService>();
        services.AddScoped<IUploadProcessor, UploadProcessor>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ILogService, LogService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IFileService, FileService>();
        services.AddScoped(typeof(IPasswordHasher<>), typeof(PasswordHasher<>));
        services.AddScoped<IDirectDbQuery, DirectDbQuery>();
        services.AddHostedService<FileStateBackupWorker>();
        services.AddHostedService<BackgroundFileService>();
        services.AddHttpContextAccessor();

        services.AddAntiforgery(options =>
        {
            options.HeaderName = "X-XSRF-TOKEN";
            options.Cookie.Name = "Antiforgery";
        });


        return services;
    }

    public static void SetPostConfig(this WebApplicationBuilder builder)
    {

        builder.Services.PostConfigure<FileUploadOptions>(options =>
        {
            options.StoragePath = Path.GetFullPath(
                Path.Combine(builder.Environment.ContentRootPath, options.StoragePath)
            );

            if (!Directory.Exists(options.StoragePath))
            {
                Directory.CreateDirectory(options.StoragePath);
            }
        });
    }
    public static IServiceCollection SwaggerConfig(this IServiceCollection services)
    {

        services.AddSwaggerGen(c =>
        {
            //   c.DocInclusionPredicate((docName, apiDesc) =>
            //   {
            //       if (apiDesc.ActionDescriptor is
            //               ControllerActionDescriptor controllerDescriptor)
            //       {
            //           // Check if the controller has the [ApiController] attribute
            //           var hasApiControllerAttribute = controllerDescriptor.ControllerTypeInfo
            //               .GetCustomAttributes(typeof(ApiControllerAttribute), true)
            //               .Any();
            //           var hasApiRoute = apiDesc.RelativePath != null && apiDesc.RelativePath.StartsWith("api/");

            //           return hasApiControllerAttribute || hasApiRoute;
            //       }

            //       return false;
            //   });
        });
        return services;
    }
}
