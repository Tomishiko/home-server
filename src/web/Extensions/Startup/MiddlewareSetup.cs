using Microsoft.AspNetCore.Antiforgery;

namespace web.Extensions;


public static class MiddlewareSetup
{
    public static IApplicationBuilder UseFailedRequestLogging(this IApplicationBuilder app)
    {

        // TODO: remove dirty logging
        app.UseStatusCodePages(context =>
        {
            var response = context.HttpContext.Response;
            var req = context.HttpContext.Request;
            Console.WriteLine($"Failed request: {req.Method} {req.Path} => {response.StatusCode}");

            return Task.CompletedTask;
        });


        return app;
    }
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            context.Response.Headers.Add("X-Frame-Options", "SAMEORIGIN");
            await next();
        });
        return app;
    }
}
