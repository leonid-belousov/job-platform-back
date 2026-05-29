namespace JobPlatform.BLL.CQRS.Candidates.DTO;

public sealed record ResumeDto(Guid Id, Guid CandidateProfileId, string Title, Guid? FileId, string Status, bool IsDefault, DateTimeOffset CreatedAt);