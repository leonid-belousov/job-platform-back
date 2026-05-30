namespace JobPlatform.BLL.Common.Models;

public sealed record PagedResult<T>(
    IReadOnlyCollection<T> Items,
    int Total,
    int Page,
    int PageSize);