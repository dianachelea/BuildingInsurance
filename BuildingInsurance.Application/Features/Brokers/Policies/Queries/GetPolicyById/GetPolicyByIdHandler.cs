using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Brokers.Policies.Common.Verifiers.Abstractions;
using BuildingInsurance.Application.Features.Brokers.Policies.Queries.GetPolicyById;
using BuildingInsurance.Application.Features.Common.Abstractions;
using BuildingInsurance.Application.Features.Common.Dtos;
using BuildingInsurance.Application.Features.Common.Result;
using BuildingInsurance.Domain.Enums;
using MediatR;

namespace BuildingInsurance.Application.Features.Brokers.Policies.Queries
{
    public sealed class GetPolicyByIdHandler : IRequestHandler<GetPolicyByIdQuery, Result<PolicyDetailsDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGeographyCachingService _geographyHostedService;
        private readonly IClientBuildingVerifier _clientBuildingVerifier;
        private readonly IPolicyPricingService _policyPricingService;
        private readonly IClock _clock;

        public GetPolicyByIdHandler(IUnitOfWork unitOfWork, IGeographyCachingService geographyHostedService, IClientBuildingVerifier clientBuildingVerifier, IPolicyPricingService policyPricingService, IClock clock)
        {
            _unitOfWork = unitOfWork;
            _geographyHostedService = geographyHostedService;
            _clientBuildingVerifier = clientBuildingVerifier;
            _policyPricingService = policyPricingService;
            _clock = clock;
        }

        public async Task<Result<PolicyDetailsDto>> Handle(GetPolicyByIdQuery request, CancellationToken cancellationToken)
        {
            var policy = await _unitOfWork.Policies.GetDetailsAsync(request.PolicyId, cancellationToken);
            if (policy is null)
                return Result<PolicyDetailsDto>.Failure($"Policy with ID {request.PolicyId} not found.", ErrorType.NotFound);

            var clientBuildingResult = await _clientBuildingVerifier.GetAndVerifyAsync(policy.ClientId, policy.BuildingId, cancellationToken);
            if (!clientBuildingResult.IsSuccess)
                return Result<PolicyDetailsDto>.Failure(clientBuildingResult.Error, clientBuildingResult.ErrorType);
            var (client, building) = clientBuildingResult.Value;
            
            var currency = await _unitOfWork.Currencies.GetByIdAsync(policy.CurrencyId, cancellationToken);
            if (currency is null)
                return Result<PolicyDetailsDto>.Failure("Currency not found for policy.", ErrorType.NotFound);

            if (!_geographyHostedService.TryGet(building.CityId, out var city, out var county, out var country))
                return Result<PolicyDetailsDto>.Failure("Geography not found for building.", ErrorType.NotFound);
            
            var nowUtc = _clock.UtcNow;
            
            var pricing = await _policyPricingService.CalculateAsync(policy, building, cancellationToken);

            var isDraft = policy.PolicyStatus == PolicyStatus.Draft;

            var dto = new PolicyDetailsDto
            {
                Id = policy.Id,
                PolicyNumber = policy.PolicyNumber,
                PolicyStatus = policy.PolicyStatus,
                StartDate = policy.StartDate,
                EndDate = policy.EndDate,
                BasePremium = policy.BasePremium,
                FinalPremium = isDraft ? 0m : policy.FinalPremium,
                FinalPremiumInBaseCurrency = isDraft ? 0m : policy.FinalPremiumInBaseCurrency,
                EstimatedFinalPremium = isDraft ? pricing.FinalPremium : policy.FinalPremium,
                Currency = currency.Code,
                CancellationEffectiveDate = policy.CancellationEffectiveDate,

                Client = new ClientDetailsDto
                {
                    Id = client.Id,
                    FullName = client.FullName,
                    Type = client.Type,
                    PersonalIdentificationNumber = client.PersonalIdentificationNumber,
                    CompanyRegistrationNumber = client.CompanyRegistrationNumber,
                    Email = client.ContactInfo.Email,
                    Phone = client.ContactInfo.Phone,
                    Address = client.ContactInfo.Address == null ? null : new AddressDto
                    {
                        Street = client.ContactInfo.Address.Street,
                        Number = client.ContactInfo.Address.Number
                    }
                },

                Building = new BuildingDetailsDto
                {
                    Id = building.Id,
                    ClientId = building.ClientId,
                    Street = building.Address.Street,
                    Number = building.Address.Number,
                    City = city,
                    County = county,
                    Country = country,
                    ConstructionYear = building.ConstructionYear,
                    Type = building.Type,
                    NumberOfFloors = building.NumberOfFloors,
                    SurfaceArea = building.SurfaceArea,
                    InsuredValue = building.InsuredValue
                },

                AppliedFees = isDraft
                    ? pricing.Fees.Select(f => new PolicyAppliedFeeDto
                    {
                        FeeConfigurationId = f.FeeConfigurationId,
                        FeeName = f.FeeName,
                        Percentage = f.Percentage,
                        AppliedAtUtc = nowUtc
                    }).ToList()
                    : policy.AppliedFees.Select(f => new PolicyAppliedFeeDto
                    {
                        FeeConfigurationId = f.FeeConfigurationId,
                        FeeName = f.FeeName,
                        Percentage = f.Percentage,
                        AppliedAtUtc = f.AppliedAtUtc
                    }).ToList(),

                AppliedRiskFactors = isDraft
                    ? pricing.Risks.Select(r => new PolicyAppliedRiskFactorDto
                    {
                        RiskFactorConfigurationId = r.RiskFactorConfigurationId,
                        Level = r.Level,
                        ReferenceId = r.ReferenceId,
                        BuildingType = r.BuildingType,
                        AdjustmentPercentage = r.AdjustmentPercentage,
                        AppliedAtUtc = nowUtc
                    }).ToList()
                    : policy.AppliedRiskFactors.Select(r => new PolicyAppliedRiskFactorDto
                    {
                        RiskFactorConfigurationId = r.RiskFactorConfigurationId,
                        Level = r.Level,
                        ReferenceId = r.ReferenceId,
                        BuildingType = r.BuildingType,
                        AdjustmentPercentage = r.AdjustmentPercentage,
                        AppliedAtUtc = r.AppliedAtUtc
                    }).ToList()
            };

            return Result<PolicyDetailsDto>.Success(dto);
        }
    }
}