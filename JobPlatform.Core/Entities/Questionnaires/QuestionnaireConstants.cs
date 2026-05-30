namespace JobPlatform.Core.Entities.Questionnaires;

public static class QuestionnaireEntityTypes
{
    public const string Candidate = "candidate";
    public const string Vacancy = "vacancy";
    public const string Company = "company";
    public const string Application = "application";
    public const string CrmLead = "crm_lead";

    public static readonly IReadOnlyCollection<string> All = new[] { Candidate, Vacancy, Company, Application, CrmLead };
}

public static class QuestionnaireQuestionTypes
{
    public const string Text = "text";
    public const string TextArea = "textarea";
    public const string Number = "number";
    public const string Date = "date";
    public const string SingleChoice = "single_choice";
    public const string MultipleChoice = "multiple_choice";
    public const string Checkbox = "checkbox";
    public const string Select = "select";
    public const string File = "file";
    public const string Scale = "scale";

    public static readonly IReadOnlyCollection<string> All = new[] { Text, TextArea, Number, Date, SingleChoice, MultipleChoice, Checkbox, Select, File, Scale };
    public static readonly IReadOnlyCollection<string> OptionBased = new[] { SingleChoice, MultipleChoice, Select };
}
