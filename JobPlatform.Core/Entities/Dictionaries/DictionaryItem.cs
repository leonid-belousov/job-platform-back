using JobPlatform.Core.Entities.Common;

namespace JobPlatform.Core.Entities.Dictionaries;

public sealed class DictionaryItem : BaseEntity
{
    public string Type { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
