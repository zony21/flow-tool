namespace FlowDesigner.Domain.Entities.Core;

public class Stage
{
    public Guid StageId { get; set; }
    public Guid FlowId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = StageCategories.Equipment;
    public string StageType { get; set; } = StageTypes.Auto;
    public int SortOrder { get; set; }
}
