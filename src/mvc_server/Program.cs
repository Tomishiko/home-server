using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication;
using mvc_server.Services;
using System.Text;

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
         });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

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
