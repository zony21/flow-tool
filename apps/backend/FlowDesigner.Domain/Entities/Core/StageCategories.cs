namespace FlowDesigner.Domain.Entities.Core;

public static class StageCategories
{
    public const string Person = "PERSON";
    public const string Server = "SERVER";
    public const string Plc = "PLC";
    public const string Wcs = "WCS";
    public const string Rcs = "RCS";
    public const string Agf = "AGF";
    public const string Agv = "AGV";
    public const string Equipment = "EQUIPMENT";
    public const string Other = "OTHER";

    private static readonly HashSet<string> Values = new(StringComparer.Ordinal)
    {
        Person, Server, Plc, Wcs, Rcs, Agf, Agv, Equipment, Other,
    };

    public static bool IsValid(string? value) => !string.IsNullOrWhiteSpace(value) && Values.Contains(value);

    public static string NormalizeOrDefault(string? value)
        => IsValid(value) ? value! : Equipment;
}
