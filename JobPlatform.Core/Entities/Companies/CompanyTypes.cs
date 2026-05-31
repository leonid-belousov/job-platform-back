namespace JobPlatform.Core.Entities.Companies;

public static class CompanyTypes
{
    public const string DirectEmployer = "direct_employer";
    public const string RecruitmentAgency = "recruitment_agency";

    public static readonly IReadOnlyCollection<string> All = new[]
    {
        DirectEmployer,
        RecruitmentAgency
    };

    public static bool IsValid(string? type)
        => !string.IsNullOrWhiteSpace(type)
           && All.Contains(type.Trim().ToLowerInvariant(), StringComparer.OrdinalIgnoreCase);
}
