namespace JobPlatform.BLL.CQRS.Gdpr.DTO;

public sealed record PersonalDataExportDto(
    object User,
    object? CandidateProfile,
    IReadOnlyCollection<object> LegalConsents,
    IReadOnlyCollection<object> Applications,
    IReadOnlyCollection<object> Notifications,
    DateTimeOffset ExportedAt);
