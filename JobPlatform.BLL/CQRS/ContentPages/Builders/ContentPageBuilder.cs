using System.Text.Json;
using JobPlatform.BLL.CQRS.ContentPages.DTO;
using JobPlatform.Core.Entities.Content;

namespace JobPlatform.BLL.CQRS.ContentPages.Builders;

public sealed class ContentPageBuilder
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly List<ContentPageBlockDto> _blocks = new();
    private readonly string _slug;
    private int _schemaVersion = 1;

    private ContentPageBuilder(string slug)
    {
        _slug = NormalizeSlug(slug);
    }

    public static ContentPageBuilder Create(string slug) => new(slug);

    public ContentPageBuilder WithSchemaVersion(int schemaVersion)
    {
        if (schemaVersion < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(schemaVersion), "Schema version must be greater than zero.");
        }

        _schemaVersion = schemaVersion;
        return this;
    }

    public ContentPageBuilder AddBlock(
        string type,
        string key,
        Dictionary<string, Dictionary<string, string?>> translations,
        Dictionary<string, string?>? settings = null)
    {
        if (string.IsNullOrWhiteSpace(type))
        {
            throw new ArgumentException("Block type is required.", nameof(type));
        }

        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Block key is required.", nameof(key));
        }

        if (translations.Count == 0)
        {
            throw new ArgumentException("At least one translation is required.", nameof(translations));
        }

        _blocks.Add(new ContentPageBlockDto(
            type.Trim().ToLowerInvariant(),
            key.Trim(),
            _blocks.Count,
            NormalizeTranslations(translations),
            settings ?? new Dictionary<string, string?>()));

        return this;
    }

    public ContentPageBuilder AddHero(
        string key,
        Dictionary<string, Dictionary<string, string?>> translations,
        Dictionary<string, string?>? settings = null)
        => AddBlock("hero", key, translations, settings);

    public ContentPageBuilder AddMetric(
        string key,
        Dictionary<string, Dictionary<string, string?>> translations,
        Dictionary<string, string?>? settings = null)
        => AddBlock("metric", key, translations, settings);

    public ContentPageBuilder AddCard(
        string key,
        Dictionary<string, Dictionary<string, string?>> translations,
        Dictionary<string, string?>? settings = null)
        => AddBlock("card", key, translations, settings);

    public ContentPageBuilder AddTimelineItem(
        string key,
        Dictionary<string, Dictionary<string, string?>> translations,
        Dictionary<string, string?>? settings = null)
        => AddBlock("timeline", key, translations, settings);

    public ContentPageBuilder AddText(
        string key,
        Dictionary<string, Dictionary<string, string?>> translations,
        Dictionary<string, string?>? settings = null)
        => AddBlock("text", key, translations, settings);

    public ContentPageDto Build(Guid? id = null, bool isPublished = true, DateTimeOffset? publishedAt = null,
        DateTimeOffset? updatedAt = null)
        => new(id, _slug, _schemaVersion, isPublished, publishedAt, updatedAt,
            _blocks.OrderBy(x => x.Order).ToArray());

    public string BuildJsonContent() => ToJson(_schemaVersion, _blocks);

    public static string ToJson(int schemaVersion, IEnumerable<ContentPageBlockDto> blocks)
    {
        var content = new ContentPageContentDto(
            schemaVersion,
            blocks.OrderBy(x => x.Order)
                .Select((block, index) => block with { Order = index })
                .ToArray());

        return JsonSerializer.Serialize(content, JsonOptions);
    }

    public static ContentPageDto FromEntity(ContentPage page)
    {
        var content = FromJson(page.JsonContent);

        return new ContentPageDto(
            page.Id,
            page.Slug,
            page.SchemaVersion,
            page.IsPublished,
            page.PublishedAt,
            page.UpdatedAt,
            content.Blocks);
    }

    public static ContentPageContentDto FromJson(string jsonContent)
    {
        if (string.IsNullOrWhiteSpace(jsonContent))
        {
            return new ContentPageContentDto(1, Array.Empty<ContentPageBlockDto>());
        }

        return JsonSerializer.Deserialize<ContentPageContentDto>(jsonContent, JsonOptions)
               ?? new ContentPageContentDto(1, Array.Empty<ContentPageBlockDto>());
    }

    public static ContentPageDto BuildDefaultAboutPage()
        => CreateDefaultAboutPage().Build(isPublished: true, publishedAt: DateTimeOffset.UtcNow);

    public static string BuildDefaultAboutPageJson()
        => CreateDefaultAboutPage().BuildJsonContent();

    private static ContentPageBuilder CreateDefaultAboutPage()
        => Create("about")
            .WithSchemaVersion(1)
            .AddHero("main", Translation(
                ("ru", new Dictionary<string, string?>
                {
                    ["tag"] = "О платформе",
                    ["title"] = "О нас",
                    ["subtitle"] = "Job Platform — HR-платформа для кандидатов, работодателей и рекрутеров. Страница собирается из JSON-блоков и может редактироваться через админку.",
                    ["primaryActionLabel"] = "Смотреть вакансии",
                    ["primaryActionUrl"] = "/vacancies",
                    ["secondaryActionLabel"] = "Компании",
                    ["secondaryActionUrl"] = "/companies"
                }),
                ("en", new Dictionary<string, string?>
                {
                    ["tag"] = "About platform",
                    ["title"] = "About us",
                    ["subtitle"] = "Job Platform is an HR platform for candidates, employers and recruiters. The page is assembled from JSON blocks and can be edited in the admin panel.",
                    ["primaryActionLabel"] = "View vacancies",
                    ["primaryActionUrl"] = "/vacancies",
                    ["secondaryActionLabel"] = "Companies",
                    ["secondaryActionUrl"] = "/companies"
                })))
            .AddMetric("audiences", Translation(
                ("ru", new Dictionary<string, string?> { ["value"] = "3", ["label"] = "ключевые аудитории: кандидаты, работодатели и рекрутеры" }),
                ("en", new Dictionary<string, string?> { ["value"] = "3", ["label"] = "core audiences: candidates, employers and recruiters" })))
            .AddMetric("modules", Translation(
                ("ru", new Dictionary<string, string?> { ["value"] = "12+", ["label"] = "модулей для управления подбором и откликами" }),
                ("en", new Dictionary<string, string?> { ["value"] = "12+", ["label"] = "modules for hiring and application management" })))
            .AddMetric("availability", Translation(
                ("ru", new Dictionary<string, string?> { ["value"] = "24/7", ["label"] = "ориентир на доступность сервиса после production-запуска" }),
                ("en", new Dictionary<string, string?> { ["value"] = "24/7", ["label"] = "target service availability after production launch" })))
            .AddText("what-we-do", Translation(
                ("ru", new Dictionary<string, string?> { ["title"] = "Что мы делаем", ["text"] = "Создаем единое пространство для поиска работы, публикации вакансий и ведения кандидатов по этапам подбора." }),
                ("en", new Dictionary<string, string?> { ["title"] = "What we do", ["text"] = "We create a single workspace for job search, vacancy publishing and candidate pipeline management." })))
            .AddCard("people-first", Translation(
                ("ru", new Dictionary<string, string?> { ["title"] = "Фокус на людях", ["text"] = "Интерфейс проектируется так, чтобы кандидат, HR и рекрутер понимали следующий шаг без лишней бюрократии." }),
                ("en", new Dictionary<string, string?> { ["title"] = "People first", ["text"] = "The interface is designed so candidates, HR teams and recruiters understand the next step without unnecessary bureaucracy." })),
                new Dictionary<string, string?> { ["icon"] = "users" })
            .AddCard("transparent-process", Translation(
                ("ru", new Dictionary<string, string?> { ["title"] = "Прозрачность процессов", ["text"] = "Статусы, отклики, заметки и коммуникации собираются в единую структуру для понятного процесса найма." }),
                ("en", new Dictionary<string, string?> { ["title"] = "Transparent processes", ["text"] = "Statuses, applications, notes and communication are organized into a clear hiring workflow." })),
                new Dictionary<string, string?> { ["icon"] = "shield" })
            .AddCard("product-growth", Translation(
                ("ru", new Dictionary<string, string?> { ["title"] = "Развитие продукта", ["text"] = "MVP постепенно наполняется реальными данными, аналитикой, интеграциями и полезными сценариями." }),
                ("en", new Dictionary<string, string?> { ["title"] = "Product growth", ["text"] = "The MVP will gradually gain real data, analytics, integrations and useful workflows." })),
                new Dictionary<string, string?> { ["icon"] = "sparkles" })
            .AddTimelineItem("idea", Translation(
                ("ru", new Dictionary<string, string?> { ["text"] = "Идея: создать единое пространство для поиска работы, публикации вакансий и ведения кандидатов." }),
                ("en", new Dictionary<string, string?> { ["text"] = "Idea: create a single space for job search, vacancy publishing and candidate management." })))
            .AddTimelineItem("mvp", Translation(
                ("ru", new Dictionary<string, string?> { ["text"] = "MVP: собрать базовый frontend, backend, роли, авторизацию и первые пользовательские сценарии." }),
                ("en", new Dictionary<string, string?> { ["text"] = "MVP: deliver the basic frontend, backend, roles, authentication and first user flows." })))
            .AddTimelineItem("next", Translation(
                ("ru", new Dictionary<string, string?> { ["text"] = "Следующий этап: наполнение страницы реальными материалами, метриками и интеграциями." }),
                ("en", new Dictionary<string, string?> { ["text"] = "Next stage: enrich the page with real content, metrics and integrations." })))
            .AddText("story", Translation(
                ("ru", new Dictionary<string, string?> { ["tag"] = "История", ["title"] = "От идеи до платформы для подбора", ["text"] = "Команда Job Platform объединяет опыт разработки, HR-процессов и продуктового дизайна, чтобы сделать найм проще, быстрее и прозрачнее." }),
                ("en", new Dictionary<string, string?> { ["tag"] = "Story", ["title"] = "From idea to hiring platform", ["text"] = "The Job Platform team combines software development, HR process and product design experience to make hiring simpler, faster and more transparent." })));

    private static Dictionary<string, Dictionary<string, string?>> Translation(
        params (string Language, Dictionary<string, string?> Values)[] translations)
        => translations.ToDictionary(
            x => x.Language.Trim().ToLowerInvariant(),
            x => x.Values);

    private static Dictionary<string, Dictionary<string, string?>> NormalizeTranslations(
        Dictionary<string, Dictionary<string, string?>> translations)
        => translations.ToDictionary(
            x => x.Key.Trim().ToLowerInvariant(),
            x => x.Value.ToDictionary(field => field.Key.Trim(), field => field.Value));

    private static string NormalizeSlug(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            throw new ArgumentException("Page slug is required.", nameof(slug));
        }

        return slug.Trim().ToLowerInvariant();
    }
}
