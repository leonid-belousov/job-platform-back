namespace JobPlatform.BLL.CQRS.ContentPages.DTO;

public sealed record ContentPageDto(
    Guid? Id,
    string Slug,
    int SchemaVersion,
    bool IsPublished,
    DateTimeOffset? PublishedAt,
    DateTimeOffset? UpdatedAt,
    IReadOnlyList<ContentPageBlockDto> Blocks);

public sealed record ContentPageBlockDto(
    string Type,
    string Key,
    int Order,
    Dictionary<string, Dictionary<string, string?>> Translations,
    Dictionary<string, string?> Settings);

public sealed record ContentPageContentDto(
    int SchemaVersion,
    IReadOnlyList<ContentPageBlockDto> Blocks);
