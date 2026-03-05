using web.Services;
using core.Services;
using Data.Core;
using System.Net;
using web.Extensions;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using core.Models.Generic;

namespace web;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var isDev = builder.Environment.IsDevelopment();

        builder.Services.RegisterCoreServices(builder.Configuration);
        builder.SetPostConfig();
        builder.Services.SetAuthentication();

        // MVC setup
        builder.Services
            .AddControllersWithViews(options =>
            {
                options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
            });
        //.ConfigureApiBehaviorOptions(options =>
        //{
        //    options.InvalidModelStateResponseFactory = context =>
        //    {
        //        var error = new Error
        //        (
        //             Message: "Model validation failed.",
        //             Code: "VALIDATION_ERROR"
        //         );
        //        return new BadRequestObjectResult(error);
        //    };
        //});

        if (isDev) builder.Services.SwaggerConfig();

        var app = await builder.Build().PerformServicesCheckups();

        if (!isDev)
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }
        else
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }


        app.UseFailedRequestLogging();
        //app.UseSecurityHeaders();
        app.UseHttpsRedirection();
        app.UseCors();
        app.UseStaticFiles();
        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();
        app.UseAntiforgery();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Index}/{action=Index}/{id?}");

        app.Run();
    }
}
