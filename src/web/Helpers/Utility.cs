using System;
using System.Globalization;
using System.Security.Claims;
using core.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using web.Models;

namespace web.Helpers;

public static class Utility
{
    public const long maxPartSize = 1024 * 1024 * 10;//10 Mb

    public static string BytesToStringOptimized(long value)
    {
        string suffix;
        double readable;
        switch (value)
        {
            case >= 0x1000000000000000:
                suffix = "EiB";
                readable = value >> 50;
                break;
            case >= 0x4000000000000:
                suffix = "PiB";
                readable = value >> 40;
                break;
            case >= 0x10000000000:
                suffix = "TiB";
                readable = value >> 30;
                break;
            case >= 0x40000000:
                suffix = "GiB";
                readable = value >> 20;
                break;
            case >= 0x100000:
                suffix = "MiB";
                readable = value >> 10;
                break;
            case >= 0x400:
                suffix = "KiB";
                readable = value;
                break;
            default:
                return value.ToString("0 B");
        }

        return (readable / 1024).ToString("0.## ", CultureInfo.InvariantCulture) + suffix;
    }
    public static bool IsXmlHttpRequest(string requestedWith)
    {
        if (requestedWith == "XMLHttpRequest") return true;
        else return false;
    }
    public static long? TryGetUserId(ClaimsPrincipal claimsPrincipal)
    {

        long userId;
        if (!long.TryParse(claimsPrincipal.FindFirstValue("Id"), out userId))
            return null;
        return userId;
    }
    public static ClaimsIdentity BuildClaims(UserDto user)
    {

        var claims = new List<Claim>
        {
            new (AppClaimTypes.Name, user.Username),
            new (AppClaimTypes.Role, user.Role),
            new (AppClaimTypes.Identity, user.Id.ToString())
        };

        return new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme,
            AppClaimTypes.Name,
            AppClaimTypes.Role);

    }
}
