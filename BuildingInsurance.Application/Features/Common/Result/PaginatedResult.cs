namespace BuildingInsurance.Application.Features.Common.Result
{
    public record PaginatedResult<T>(IReadOnlyList<T> Items, int TotalPages, int TotalCount);
}