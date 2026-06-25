using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Brokers.Policies.Common.Verifiers.Abstractions;
using BuildingInsurance.Application.Features.Brokers.Policies.Queries;
using BuildingInsurance.Application.Features.Brokers.Policies.Queries.GetPolicyById;
using BuildingInsurance.Application.Features.Common.Abstractions;
using BuildingInsurance.Application.Features.Common.Models;
using BuildingInsurance.Application.Features.Common.Result;
using BuildingInsurance.Domain.Enums;
using BuildingInsurance.Domain.ValueObjects;
using Moq;

namespace BuildingInsurance.Tests.Handlers.Policy
{
    public sealed class GetPolicyByIdHandlerTests
    {
        private readonly Mock<IUnitOfWork> _uow = new();
        private readonly Mock<IGeographyCachingService> _geo = new();
        private readonly Mock<IPolicyRepository> _policies = new();
        private readonly Mock<IClientBuildingVerifier> _clientBuildingVerifier = new();
        private readonly Mock<ICurrencyRepository> _currencies = new();
        private readonly GetPolicyByIdHandler _handler;
        private readonly Mock<IPolicyPricingService> _pricingService = new();
        private readonly Mock<IClock> _clock = new();
        public GetPolicyByIdHandlerTests()
        {
            _uow.SetupGet(u => u.Policies).Returns(_policies.Object);
            _uow.SetupGet(u => u.Currencies).Returns(_currencies.Object);

            _pricingService
                .Setup(s => s.CalculateAsync(
                    It.IsAny<Domain.Entities.Policies.Policy>(),
                    It.IsAny<Domain.Entities.Buildings.Building>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PolicyPricingResult(
                    123m,
                    new List<AppliedFeeSnapshot>(),
                    new List<AppliedRiskSnapshot>()));

            _clock.SetupGet(c => c.UtcNow)
                .Returns(new DateTime(2026, 02, 05, 10, 0, 0, DateTimeKind.Utc));

            _handler = new GetPolicyByIdHandler(
                _uow.Object,
                _geo.Object,
                _clientBuildingVerifier.Object,
                _pricingService.Object,
                _clock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnNotFound_When_Policy_NotFound()
        {
            var policyId = Guid.NewGuid();

            _policies.Setup(r => r.GetDetailsAsync(policyId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Domain.Entities.Policies.Policy?)null);
            
            var result = await _handler.Handle(new GetPolicyByIdQuery(policyId), CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.NotFound, result.ErrorType);
            Assert.Equal($"Policy with ID {policyId} not found.", result.Error);

            _clientBuildingVerifier.Verify(v => v.GetAndVerifyAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
            _currencies.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccess_When_All_Data_Exists()
        {
            var startUtc = new DateTime(2026, 03, 01, 0, 0, 0, DateTimeKind.Utc);
            var endUtc = new DateTime(2026, 04, 01, 0, 0, 0, DateTimeKind.Utc);

            var policy = Domain.Entities.Policies.Policy.CreateDraft(
                clientId: Guid.NewGuid(),
                buildingId: Guid.NewGuid(),
                brokerId: Guid.NewGuid(),
                currencyId: Guid.NewGuid(),
                startDate: startUtc,
                endDate: endUtc,
                basePremium: 100m);

            _policies.Setup(r => r.GetDetailsAsync(policy.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(policy);

            var client = new Domain.Entities.Clients.Client(
                policy.ClientId,
                ClientType.Individual,
                "John Doe",
                new ContactInfo("john@email.com", "0700000000"),
                "12345678901");

            var cityId = Guid.NewGuid();

            var building = new Domain.Entities.Buildings.Building(
                id: policy.BuildingId,
                clientId: policy.ClientId,
                address: new Address("Main Street", "10"),
                cityId: cityId,
                constructionYear: 2005,
                type: BuildingType.Residential,
                numberOfFloors: 2,
                surfaceArea: 120m,
                insuredValue: 150000m,
                riskIndicators: RiskIndicators.FloodZone);

            _clientBuildingVerifier
                .Setup(v => v.GetAndVerifyAsync(policy.ClientId, policy.BuildingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<(Domain.Entities.Clients.Client, Domain.Entities.Buildings.Building)>.Success((client, building)));

            var currency = new Domain.Entities.Metadata.Currency(
                id: policy.CurrencyId,
                code: "EUR",
                name: "Euro",
                exchangeRateToBase: 1m,
                isActive: true);

            _currencies.Setup(r => r.GetByIdAsync(policy.CurrencyId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(currency);

            _geo.Setup(g => g.TryGet(cityId, out It.Ref<string>.IsAny, out It.Ref<string>.IsAny, out It.Ref<string>.IsAny))
                .Returns((Guid _, out string city, out string county, out string country) =>
                {
                    city = "BUCHAREST";
                    county = "ILFOV";
                    country = "ROMANIA";
                    return true;
                });

            var result = await _handler.Handle(new GetPolicyByIdQuery(policy.Id), CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            Assert.Equal(policy.Id, result.Value!.Id);
            Assert.Equal("BUCHAREST", result.Value.Building.City);
            Assert.Equal("EUR", result.Value.Currency);
        }
    }
}