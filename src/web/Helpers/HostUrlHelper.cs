namespace web.Helpers;

public static class HostUrlHelper
{
    public static string GetBaseUrl(HttpContext context, IConfiguration config)
    {
        // 1. Check for an explicit environment variable or appsetting override
        string? configuredUrl = config["APP_BASE_URL"];
        if (!string.IsNullOrEmpty(configuredUrl))
        {
            return configuredUrl.TrimEnd('/');
        }

        // 2. Fallback to Dynamic Detect.ion if no config is set
        string host = context.Request.Headers.Host.ToString();
        if (string.IsNullOrEmpty(host))
        {
            host = context.Request.Host.Value;
        }

        string scheme = context.Request.Headers["X-Forwarded-Proto"].ToString();
        if (string.IsNullOrEmpty(scheme))
        {
            scheme = context.Request.Scheme; // "http" or "https"
        }

        return $"{scheme}://{host}";
    }
}
