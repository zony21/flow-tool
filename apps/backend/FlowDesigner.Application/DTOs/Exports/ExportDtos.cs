namespace FlowDesigner.Application.DTOs.Exports;

public sealed record MermaidExportRequest(
    string? Type,
    string? Direction,
    bool IncludeComments);

public sealed record TextExportResponse(
    string FileName,
    string Content);
