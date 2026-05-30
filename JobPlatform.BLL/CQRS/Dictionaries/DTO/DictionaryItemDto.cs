namespace JobPlatform.BLL.CQRS.Dictionaries.DTO;

public sealed record DictionaryItemDto(
    Guid Id,
    string Type,
    string Code,
    string Name,
    string? NameEn,
    string? Description,
    string? DescriptionEn,
    int SortOrder,
    bool IsActive);