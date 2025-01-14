using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using mvc_server.Services;
using System.Text;
using System.Net;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace mvc_server;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllersWithViews();

        builder.Services.AddSingleton<StreamedFileCompositor>();
        builder.Services.AddSingleton<ICoreFS, CoreFS>();
        builder.Services.AddSingleton<JWTGen>();

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
        var jwtIssuer = builder.Configuration.GetSection("JWT:issuer").Get<string>();
        var jwtKey = builder.Configuration.GetSection("JWT:key").Get<string>();

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
         .AddJwtBearer(options =>
         {
             options.TokenValidationParameters = new TokenValidationParameters
             {
                 ValidateIssuer = true,
                 ValidateAudience = true,
                 ValidateLifetime = true,
                 ValidateIssuerSigningKey = true,
                 ValidIssuer = jwtIssuer,
                 ValidAudience = jwtIssuer,
                 IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
             };
             options.Events = new JwtBearerEvents
             {
                 OnMessageReceived = ctx =>
                 {
                     Console.WriteLine($"{ctx.Request.Headers["X-Requested-With"]}");
                     if (ctx.Request.Headers["X-Requested-With"] != "XMLHttpRequest")
                     {
                         ctx.Request.Cookies.TryGetValue("AspNet.Id", out string token);
                         if (!string.IsNullOrEmpty(token))
                             ctx.Token = token;
                     }
                     return Task.CompletedTask;
                 }
             };
         });

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
                Console.WriteLine("Not authorized");
                response.Cookies.Append("returnUrl",request.Path,new CookieOptions{
                            Secure = true,
                            HttpOnly = true,
                            SameSite = SameSiteMode.Strict

                        });
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
