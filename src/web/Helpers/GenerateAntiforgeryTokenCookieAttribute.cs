using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics;

namespace web.Helpers;

public class GenerateAntiforgeryTokenCookieAttribute : ResultFilterAttribute
{
    public override void OnResultExecuting(ResultExecutingContext context)
    {
        var antiforgery = context.HttpContext.RequestServices.GetService<IAntiforgery>();
        Debug.Assert(antiforgery is not null );
        // Send the request token as a JavaScript-readable cookie
        AntiforgeryTokenSet tokens = antiforgery.GetAndStoreTokens(context.HttpContext);

        context.HttpContext.Response.Cookies.Append(
            "RequestVerificationToken",
            tokens.RequestToken,
            new CookieOptions() { HttpOnly = false });
    }

    public override void OnResultExecuted(ResultExecutedContext context)
    {
    }
}
