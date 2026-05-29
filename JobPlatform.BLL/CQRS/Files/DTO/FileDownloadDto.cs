namespace JobPlatform.BLL.CQRS.Files.DTO;

public sealed record FileDownloadDto(
    Stream Content,
    string OriginalName,
    string ContentType);
