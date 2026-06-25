using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Common.Dtos;
using BuildingInsurance.Application.Features.Common.Mapping;
using BuildingInsurance.Application.Features.Common.Result;
using MediatR;

namespace BuildingInsurance.Application.Features.Brokers.Policies.Queries.ListPolicies
{
    public sealed class ListPoliciesHandler : IRequestHandler<ListPoliciesQuery, Result<ListPoliciesResponse>>
    {
        private readonly IPolicyRepository _policyRepository;

        public ListPoliciesHandler(IPolicyRepository policyRepository)
        {
            _policyRepository = policyRepository;
        }

        public async Task<Result<ListPoliciesResponse>> Handle(ListPoliciesQuery request, CancellationToken cancellationToken)
        {
            var (items, totalCount) = await _policyRepository.SearchPagedAsync(
                clientId: request.ClientId,
                brokerId: request.BrokerId,
                status: request.Status.MapToDomainPolicyStatusOptional(),
                startDate: null,
                endDate: null,
                page: request.Page,
                pageSize: request.PageSize,
                ct: cancellationToken);

            var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)request.PageSize);

            var dtos = items.Select(p => new PolicyDto
            {
                Id = p.Id,
                PolicyNumber = p.PolicyNumber,
                ClientId = p.ClientId,
                BuildingId = p.BuildingId,
                BrokerId = p.BrokerId,
                CurrencyId = p.CurrencyId,
                PolicyStatus = p.PolicyStatus,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                BasePremium = p.BasePremium,
                FinalPremium = p.FinalPremium,
                CancellationEffectiveDate = p.CancellationEffectiveDate
            }).ToList();

            return Result<ListPoliciesResponse>.Success(new ListPoliciesResponse(dtos, totalPages, totalCount));
        }
    }
}