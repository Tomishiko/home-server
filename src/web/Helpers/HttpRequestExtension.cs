namespace web.Helpers;

public static class HttpRequestExtensions
{
    public static bool IsJsonRequest(this HttpRequest request)
    {
        var acceptHeader = request.Headers["Accept"].ToString();

        return acceptHeader.Contains("application/json") ||
               request.Path.StartsWithSegments("/api");
    }
}
