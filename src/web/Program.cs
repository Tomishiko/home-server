using web.Services;
using core.Services;
using Data.Core;
using System.Net;
using web.Extensions;

namespace web;

public static class Program
{
    static void MyHandler(object sender, UnhandledExceptionEventArgs args)
    {
        Exception e = (Exception)args.ExceptionObject;
        Console.WriteLine("MyHandler caught : " + e.Message);
        Console.WriteLine("Runtime terminating: {0}", args.IsTerminating);
        Console.WriteLine("StackTrace: " + e.StackTrace);
        Console.WriteLine(args.ExceptionObject.ToString());
    }
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        AppDomain.CurrentDomain.UnhandledException += MyHandler;
        builder.Services.Startup(builder.Configuration);
        // Add services to the container.
        builder.Services.AddControllersWithViews();
        //builder.Services.AddScoped<Irepos>

        //TODO: AntiForgery token
        // builder.Services.AddRazorPages(options =>
        // {
        //     options.Conventions
        //         .AddPageApplicationModelConvention("/Home",
        //             model =>
        //             {
        //                 model.Filters.Add(
        //                     new GenerateAntiforgeryTokenCookieAttribute());
        //                 model.Filters.Add(
        //                     new DisableFormValueModelBindingAttribute());
        //             });
        // });
        //var jwtIssuer = builder.Configuration.GetSection("JWT:issuer").Get<string>();
        //var jwtKey = builder.Configuration.GetSection("JWT:key").Get<string>();


        var app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }
        app.UseStatusCodePages(async context =>
        {
            var request = context.HttpContext.Request;
            var response = context.HttpContext.Response;
            if (response.StatusCode == (int)HttpStatusCode.Unauthorized //TODO change this retarded shit to check for ajax requests
                    && request.Headers.Accept.Any(x => x.Contains("text/html")))
            {
                response.Redirect("/login");

            }
        });

        app.UseAuthentication();
        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseCors();
        app.UseRouting();

        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Index}/{action=Index}/{id?}");

        app.Run();
    }
}
