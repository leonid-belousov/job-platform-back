namespace JobPlatform.BLL.CQRS.Dictionaries.DTO;

public sealed record DictionaryItemDto(
    Guid Id,
    string Type,
    string Code,
    string Name,
    string? Description,
    int SortOrder,
    bool IsActive);
