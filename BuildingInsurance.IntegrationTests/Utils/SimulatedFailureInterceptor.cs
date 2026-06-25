using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BuildingInsurance.IntegrationTests.Utils
{
    public sealed class SimulatedFailureInterceptor : SaveChangesInterceptor
    {
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            throw new Exception("Simulated database failure during Commit!");
        }
    }
}