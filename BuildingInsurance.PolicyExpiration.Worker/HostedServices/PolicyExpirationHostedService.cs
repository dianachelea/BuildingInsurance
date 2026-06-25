using BuildingInsurance.Application.Features.Common.Abstractions;

namespace BuildingInsurance.PolicyExpiration.Worker.HostedServices
{
    public sealed class PolicyExpirationHostedService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IClock _clock;
        private readonly ILogger<PolicyExpirationHostedService> _logger;

        public PolicyExpirationHostedService(IServiceScopeFactory scopeFactory, IClock clock, ILogger<PolicyExpirationHostedService> logger)
        {
            _scopeFactory = scopeFactory;
            _clock = clock;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("PolicyExpirationHostedService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var runner = scope.ServiceProvider.GetRequiredService<IPolicyExpirationService>();

                    await runner.RunOnceAsync(_clock.UtcNow, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while expiring policies.");
                }

                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            }
        }
    }
}