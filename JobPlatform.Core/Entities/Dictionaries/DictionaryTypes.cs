namespace JobPlatform.Core.Entities.Dictionaries;

public static class DictionaryTypes
{
    public const string City = "city";
    public const string Country = "country";
    public const string Currency = "currency";
    public const string EmploymentType = "employment_type";
    public const string WorkFormat = "work_format";
    public const string ExperienceLevel = "experience_level";
    public const string Specialization = "specialization";
    public const string Industry = "industry";
    public const string Skill = "skill";
    public const string VacancyStatus = "vacancy_status";
    public const string ApplicationStatus = "application_status";

    public static readonly IReadOnlyCollection<string> All = new[]
    {
        City,
        Country,
        Currency,
        EmploymentType,
        WorkFormat,
        ExperienceLevel,
        Specialization,
        Industry,
        Skill,
        VacancyStatus,
        ApplicationStatus
    };
}
