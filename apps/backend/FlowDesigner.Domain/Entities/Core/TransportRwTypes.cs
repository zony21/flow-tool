namespace FlowDesigner.Domain.Entities.Core;

public static class TransportRwTypes
{
    public const string None = "NONE";
    public const string Read = "READ";
    public const string Write = "WRITE";

    public static bool IsValid(string? value)
    {
        return value is null or None or Read or Write;
    }

    public static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToUpperInvariant();
    }
}
