using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Brokers.Policies.Queries.ListPolicies;
using BuildingInsurance.Application.Features.Common.Contracts.Enums;
using BuildingInsurance.Domain.Enums;
using Moq;

namespace BuildingInsurance.Tests.Handlers.Policy
{
    public sealed class ListPoliciesHandlerTests
    {
        private readonly Mock<IPolicyRepository> _policies = new();
        private readonly ListPoliciesHandler _handler;

        public ListPoliciesHandlerTests()
        {
            _handler = new ListPoliciesHandler(_policies.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccess_With_Empty_List_When_No_Items()
        {
            _policies.Setup(r => r.SearchPagedAsync(
                    It.IsAny<Guid?>(),
                    It.IsAny<Guid?>(),
                    It.IsAny<PolicyStatus?>(),
                    null,
                    null,
                    1,
                    10,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((new List<Domain.Entities.Policies.Policy>(), 0));

            var result = await _handler.Handle(new ListPoliciesQuery
            {
                ClientId = null,
                BrokerId = null,
                Status = PolicyStatusContract.Active,
                Page = 1,
                PageSize = 10
            }, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Empty(result.Value!.Items);
            Assert.Equal(0, result.Value.TotalPages);
            Assert.Equal(0, result.Value.TotalCount);
        }

        [Fact]
        public async Task Handle_ShouldReturnTotalPages_Correctly()
        {
            var policy1 = Domain.Entities.Policies.Policy.CreateDraft(
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
                new DateTime(2026, 03, 01, 0, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 04, 01, 0, 0, 0, DateTimeKind.Utc),
                100m);

            var policy2 = Domain.Entities.Policies.Policy.CreateDraft(
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
                new DateTime(2026, 03, 01, 0, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 04, 01, 0, 0, 0, DateTimeKind.Utc),
                200m);

            _policies.Setup(r => r.SearchPagedAsync(
                    It.IsAny<Guid?>(),
                    It.IsAny<Guid?>(),
                    It.IsAny<PolicyStatus?>(),
                    null,
                    null,
                    2,
                    10,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((new List<Domain.Entities.Policies.Policy> { policy1, policy2 }, 21));

            var result = await _handler.Handle(new ListPoliciesQuery
            {
                ClientId = null,
                BrokerId = null,
                Status = PolicyStatusContract.Active,
                Page = 2,
                PageSize = 10
            }, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            Assert.Equal(3, result.Value!.TotalPages);
            Assert.Equal(21, result.Value.TotalCount);
            Assert.Equal(2, result.Value.Items.Count);
        }
    }
}