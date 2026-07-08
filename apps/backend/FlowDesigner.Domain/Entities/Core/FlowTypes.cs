namespace FlowDesigner.Domain.Entities.Core;

public static class FlowTypes
{
    public const string Normal = "NORMAL";
    public const string Transport = "TRANSPORT";

    public static bool IsValid(string? value)
    {
        return string.Equals(value, Normal, StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, Transport, StringComparison.OrdinalIgnoreCase);
    }

    public static string NormalizeOrDefault(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Normal;
        }

        var normalized = value.Trim().ToUpperInvariant();
        return IsValid(normalized) ? normalized : Normal;
    }
}
