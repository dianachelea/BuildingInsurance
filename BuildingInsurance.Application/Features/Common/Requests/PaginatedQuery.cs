namespace BuildingInsurance.Application.Features.Common.Requests
{
    public abstract record PaginatedQuery
    {
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 10;
    }
}