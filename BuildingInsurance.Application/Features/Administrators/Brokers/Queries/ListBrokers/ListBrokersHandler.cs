using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Common.Dtos;
using BuildingInsurance.Application.Features.Common.Result;
using MediatR;

namespace BuildingInsurance.Application.Features.Administrators.Brokers.Queries.ListBrokers
{
    public sealed class ListBrokersHandler : IRequestHandler<ListBrokersQuery, Result<ListBrokersResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public ListBrokersHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<ListBrokersResponse>> Handle(ListBrokersQuery request, CancellationToken cancellationToken)
        {
            var (brokers, totalCount) = await _unitOfWork.Brokers.SearchPagedAsync(request.Name, request.IsActive, request.Page, request.PageSize, cancellationToken);

            var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)request.PageSize);

            var items = brokers.Select(b => new BrokerDto
            {
                Id = b.Id,
                BrokerCode = b.BrokerCode,
                FullName = b.FullName,
                Email = b.ContactInfo.Email,
                Phone = b.ContactInfo.Phone,
                BrokerStatus = b.BrokerStatus,
                CommissionPercentage = b.CommissionPercentage
            }).ToList();

            return Result<ListBrokersResponse>.Success(new ListBrokersResponse(items, totalPages, totalCount));
        }
    }
}