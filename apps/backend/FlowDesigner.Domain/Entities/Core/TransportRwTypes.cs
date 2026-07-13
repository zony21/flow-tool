namespace FlowDesigner.Domain.Entities.Core;

public static class TransportRwTypes
{
    public const string None = "NONE";
    public const string Read = "READ";
    public const string Write = "WRITE";

    public static bool IsValid(string? value)
    {
        var normalized = Normalize(value);
        return normalized is None or Read or Write;
    }

    public static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToUpperInvariant();
    }

    public static string NormalizeOrDefault(string? value)
    {
        var normalized = Normalize(value);
        return IsValid(normalized) ? normalized! : None;
    }
}
