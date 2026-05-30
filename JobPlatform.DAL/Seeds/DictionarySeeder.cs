using JobPlatform.Core.Entities.Dictionaries;
using JobPlatform.DAL.Context;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.DAL.Seeds;

public class DictionarySeeder
{
    private readonly AppDbContext db;

    public DictionarySeeder(AppDbContext db)
    {
        this.db = db;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var items = new[]
        {
            Item(DictionaryTypes.Country, "ru", "Россия", 10),
            Item(DictionaryTypes.Country, "kz", "Казахстан", 20),
            Item(DictionaryTypes.Country, "by", "Беларусь", 30),

            Item(DictionaryTypes.City, "moscow", "Москва", 10),
            Item(DictionaryTypes.City, "saint-petersburg", "Санкт-Петербург", 20),
            Item(DictionaryTypes.City, "novosibirsk", "Новосибирск", 30),
            Item(DictionaryTypes.City, "ekaterinburg", "Екатеринбург", 40),
            Item(DictionaryTypes.City, "kazan", "Казань", 50),
            Item(DictionaryTypes.City, "remote", "Удаленно", 999),

            Item(DictionaryTypes.Currency, "rub", "RUB", 10),
            Item(DictionaryTypes.Currency, "usd", "USD", 20),
            Item(DictionaryTypes.Currency, "eur", "EUR", 30),

            Item(DictionaryTypes.EmploymentType, "full_time", "Полная занятость", 10),
            Item(DictionaryTypes.EmploymentType, "part_time", "Частичная занятость", 20),
            Item(DictionaryTypes.EmploymentType, "project", "Проектная работа", 30),
            Item(DictionaryTypes.EmploymentType, "internship", "Стажировка", 40),

            Item(DictionaryTypes.WorkFormat, "office", "Офис", 10),
            Item(DictionaryTypes.WorkFormat, "remote", "Удаленно", 20),
            Item(DictionaryTypes.WorkFormat, "hybrid", "Гибрид", 30),

            Item(DictionaryTypes.ExperienceLevel, "no_experience", "Без опыта", 10),
            Item(DictionaryTypes.ExperienceLevel, "junior", "Junior", 20),
            Item(DictionaryTypes.ExperienceLevel, "middle", "Middle", 30),
            Item(DictionaryTypes.ExperienceLevel, "senior", "Senior", 40),
            Item(DictionaryTypes.ExperienceLevel, "lead", "Lead", 50),

            Item(DictionaryTypes.Specialization, "backend", "Backend-разработка", 10),
            Item(DictionaryTypes.Specialization, "frontend", "Frontend-разработка", 20),
            Item(DictionaryTypes.Specialization, "qa", "QA / Тестирование", 30),
            Item(DictionaryTypes.Specialization, "analytics", "Аналитика", 40),
            Item(DictionaryTypes.Specialization, "hr", "HR / Рекрутинг", 50),
            Item(DictionaryTypes.Specialization, "sales", "Продажи", 60),

            Item(DictionaryTypes.Industry, "it", "Информационные технологии", 10),
            Item(DictionaryTypes.Industry, "fintech", "Финансы и FinTech", 20),
            Item(DictionaryTypes.Industry, "retail", "Ритейл", 30),
            Item(DictionaryTypes.Industry, "manufacturing", "Производство", 40),
            Item(DictionaryTypes.Industry, "hr_agency", "Кадровое агентство", 50),

            Item(DictionaryTypes.Skill, "csharp", "C#", 10),
            Item(DictionaryTypes.Skill, "aspnet-core", "ASP.NET Core", 20),
            Item(DictionaryTypes.Skill, "react", "React", 30),
            Item(DictionaryTypes.Skill, "typescript", "TypeScript", 40),
            Item(DictionaryTypes.Skill, "postgresql", "PostgreSQL", 50),

            Item(DictionaryTypes.VacancyStatus, "draft", "Черновик", 10),
            Item(DictionaryTypes.VacancyStatus, "published", "Опубликована", 20),
            Item(DictionaryTypes.VacancyStatus, "archived", "Архив", 30),
            Item(DictionaryTypes.VacancyStatus, "closed", "Закрыта", 40),

            Item(DictionaryTypes.ApplicationStatus, "sent", "Отправлен", 10),
            Item(DictionaryTypes.ApplicationStatus, "viewed", "Просмотрен", 20),
            Item(DictionaryTypes.ApplicationStatus, "in_progress", "В работе", 30),
            Item(DictionaryTypes.ApplicationStatus, "interview", "Приглашение на собеседование", 40),
            Item(DictionaryTypes.ApplicationStatus, "rejected", "Отказ", 50),
            Item(DictionaryTypes.ApplicationStatus, "accepted", "Принят", 60),
            Item(DictionaryTypes.ApplicationStatus, "closed", "Закрыт", 70)
        };

        foreach (var item in items)
        {
            var existing = await db.Set<DictionaryItem>().FirstOrDefaultAsync(x => x.Type == item.Type && x.Code == item.Code, cancellationToken);
            if (existing is null)
            {
                await db.Set<DictionaryItem>().AddAsync(item, cancellationToken);
            }
            else
            {
                existing.Name = item.Name;
                existing.SortOrder = item.SortOrder;
                existing.IsActive = true;
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private DictionaryItem Item(string type, string code, string name, int sortOrder)
        => new()
        {
            Type = type,
            Code = code,
            Name = name,
            SortOrder = sortOrder,
            IsActive = true
        };
}