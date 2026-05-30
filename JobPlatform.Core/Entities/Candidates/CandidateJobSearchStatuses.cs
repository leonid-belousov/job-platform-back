namespace JobPlatform.Core.Entities.Candidates;

public static class CandidateJobSearchStatuses
{
    public const string ActiveSearch = "active_search";
    public const string NotSearching = "not_searching";

    public static readonly IReadOnlyCollection<string> All = new[]
    {
        ActiveSearch,
        NotSearching
    };
}