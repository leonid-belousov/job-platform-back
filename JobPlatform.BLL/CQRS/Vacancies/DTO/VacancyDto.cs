namespace JobPlatform.BLL.CQRS.Vacancies.DTO;

public sealed record VacancyDto(Guid Id, string Title, string? City, decimal? SalaryFrom, decimal? SalaryTo, string Status);