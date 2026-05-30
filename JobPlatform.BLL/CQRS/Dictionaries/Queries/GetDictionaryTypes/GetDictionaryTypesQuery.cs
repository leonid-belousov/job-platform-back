using JobPlatform.BLL.CQRS.Dictionaries.DTO;
using JobPlatform.Core.Entities.Dictionaries;
using MediatR;

namespace JobPlatform.BLL.CQRS.Dictionaries.Queries.GetDictionaryTypes;

public sealed record GetDictionaryTypesQuery : IRequest<IReadOnlyCollection<DictionaryTypeDto>>
{
    public class
        GetDictionaryTypesQueryHandler : IRequestHandler<GetDictionaryTypesQuery,
        IReadOnlyCollection<DictionaryTypeDto>>
    {
        public Task<IReadOnlyCollection<DictionaryTypeDto>> Handle(GetDictionaryTypesQuery request,
            CancellationToken cancellationToken)
        {
            IReadOnlyCollection<DictionaryTypeDto> result = new[]
            {
                new DictionaryTypeDto(DictionaryTypes.City, "Города"),
                new DictionaryTypeDto(DictionaryTypes.Country, "Страны"),
                new DictionaryTypeDto(DictionaryTypes.Currency, "Валюты"),
                new DictionaryTypeDto(DictionaryTypes.EmploymentType, "Типы занятости"),
                new DictionaryTypeDto(DictionaryTypes.WorkFormat, "Форматы работы"),
                new DictionaryTypeDto(DictionaryTypes.ExperienceLevel, "Уровни опыта"),
                new DictionaryTypeDto(DictionaryTypes.Specialization, "Специализации"),
                new DictionaryTypeDto(DictionaryTypes.Industry, "Отрасли"),
                new DictionaryTypeDto(DictionaryTypes.Skill, "Навыки"),
                new DictionaryTypeDto(DictionaryTypes.VacancyStatus, "Статусы вакансий"),
                new DictionaryTypeDto(DictionaryTypes.ApplicationStatus, "Статусы откликов")
            };

            return Task.FromResult(result);
        }
    }
}