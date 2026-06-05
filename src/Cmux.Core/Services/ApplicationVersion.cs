namespace Cmux.Core.Services;

public static class ApplicationVersion
{
    public static Version Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return new Version(0, 0);

        value = value.Trim().TrimStart('v', 'V');
        if (Version.TryParse(value, out var version))
            return version;

        var digits = new string(value.TakeWhile(ch => char.IsDigit(ch) || ch == '.').ToArray());
        return Version.TryParse(digits, out version) ? version : new Version(0, 0);
    }

    public static bool IsNewer(string latest, string current)
    {
        return Parse(latest) > Parse(current);
    }

    public static string NormalizeTag(string? tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
            return "0.0.0";

        return tag.Trim().TrimStart('v', 'V');
    }
}
