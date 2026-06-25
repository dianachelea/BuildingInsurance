using BuildingInsurance.Domain.Events;

namespace BuildingInsurance.Tests.DomainEvents
{
    public class PolicyCancelledTests
    {
        [Fact]
        public void Constructor_ShouldSetProperties_Correctly()
        {
            var policyId = Guid.NewGuid();
            var reason = "Client request";
            var effectiveDate = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);

            var before = DateTime.UtcNow;

            var evt = new PolicyCancelled(policyId, reason, effectiveDate);

            var after = DateTime.UtcNow;

            Assert.Equal(policyId, evt.PolicyId);
            Assert.Equal(reason, evt.Reason);
            Assert.Equal(effectiveDate, evt.EffectiveDate);

            Assert.True(evt.OccurredOn >= before);
            Assert.True(evt.OccurredOn <= after);
            Assert.Equal(DateTimeKind.Utc, evt.OccurredOn.Kind);
        }

        [Fact]
        public void OccurredOn_ShouldBeSetAutomatically_AndBeRecent()
        {
            var policyId = Guid.NewGuid();
            var reason = "Non-payment";
            var effectiveDate = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc);

            var evt = new PolicyCancelled(policyId, reason, effectiveDate);

            var diff = DateTime.UtcNow - evt.OccurredOn;
            Assert.True(diff.TotalSeconds < 2);
        }
    }
}