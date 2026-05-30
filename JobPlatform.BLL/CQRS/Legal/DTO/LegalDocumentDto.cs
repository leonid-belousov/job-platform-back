namespace JobPlatform.BLL.CQRS.Legal.DTO;

public sealed record LegalDocumentDto(
    Guid Id,
    string Type,
    string Version,
    string Language,
    string Title,
    string Content,
    bool IsActive,
    DateTimeOffset PublishedAt);