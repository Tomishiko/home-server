using Microsoft.Extensions.FileProviders;
using mvc_server.Helpers;
using mvc_server.Service;
using mvc_server.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<StreamedFileCompositor>();
builder.Services.AddSingleton<ICoreFS, CoreFS>();
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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseCors();
app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
