namespace JobPlatform.BLL.CQRS.Files.DTO;

public sealed record FileDto(
    Guid Id,
    string OriginalName,
    string ContentType,
    long SizeBytes,
    Guid UploadedByUserId,
    DateTimeOffset CreatedAt);