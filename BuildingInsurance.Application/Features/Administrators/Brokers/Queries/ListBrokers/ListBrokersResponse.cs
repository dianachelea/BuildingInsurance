using BuildingInsurance.Application.Features.Common.Dtos;
using BuildingInsurance.Application.Features.Common.Result;

namespace BuildingInsurance.Application.Features.Administrators.Brokers.Queries.ListBrokers
{
    public sealed record ListBrokersResponse : PaginatedResult<BrokerDto>
    {
        public ListBrokersResponse(IReadOnlyList<BrokerDto> Items, int TotalPages, int TotalCount) : base(Items, TotalPages, TotalCount)
        {
        }
    }
}