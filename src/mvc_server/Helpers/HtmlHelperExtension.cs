using System;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace mvc_server.Helpers;

public static class HtmlHelperExtension
{
    public static string ActiveClass(this IHtmlHelper htmlHelper, string route)
    {
        var routeData = htmlHelper.ViewContext.RouteData;

        var pageRoute = routeData.Values["action"].ToString();

        return route == pageRoute ? "active" : "";
    }
}
