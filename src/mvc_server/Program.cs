using mvc_server.Services;
using System.Net;
using mvc_server.Extensions;

namespace mvc_server;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.SetOptions();
        // Add services to the container.
        builder.Services.AddControllersWithViews();
        builder.Services.AddSingleton<StreamedFileCompositor>();
        builder.Services.AddSingleton<ICoreFS, CoreFS>();
        builder.Services.AddSingleton<JWTGen>();
        builder.Services.SetAuthentication(builder.Configuration);

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
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }
}
