using System.Diagnostics.CodeAnalysis;

namespace core.utils.extensions;

public static class Extensions
{

    public static bool IsNullOrEmpty([NotNullWhen(false)] this string? param)
    {
        return string.IsNullOrEmpty(param);
    }
}

