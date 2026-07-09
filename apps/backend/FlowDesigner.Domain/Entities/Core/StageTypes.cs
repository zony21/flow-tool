namespace FlowDesigner.Domain.Entities.Core;

public static class StageTypes
{
    public const string Auto = "AUTO";
    public const string Manual = "MANUAL";

    public static bool IsValid(string? value)
    {
        return string.Equals(value, Auto, StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, Manual, StringComparison.OrdinalIgnoreCase);
    }

    public static string NormalizeOrDefault(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Auto;
        }

        var normalized = value.Trim().ToUpperInvariant();
        return IsValid(normalized) ? normalized : Auto;
    }
}
