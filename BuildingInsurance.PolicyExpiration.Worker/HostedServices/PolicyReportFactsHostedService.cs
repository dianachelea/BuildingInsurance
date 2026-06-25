using BuildingInsurance.Application.Features.Common.Abstractions;

namespace BuildingInsurance.PolicyExpiration.Worker.HostedServices
{
    public sealed class PolicyReportFactsHostedService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IClock _clock;
        private readonly ILogger<PolicyReportFactsHostedService> _logger;

        public PolicyReportFactsHostedService(IServiceScopeFactory scopeFactory, IClock clock, ILogger<PolicyReportFactsHostedService> logger)
        {
            _scopeFactory = scopeFactory;
            _clock = clock;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("PolicyReportFactsHostedService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var runner = scope.ServiceProvider.GetRequiredService<IPolicyReportFactsMaterializer>();

                    await runner.RunOnceAsync(_clock.UtcNow, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("PolicyReportFactsHostedService is stopping.");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while materializing policy report facts.");
                }

                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("PolicyReportFactsHostedService is stopping.");
                    break;
                }
            }
        }
    }
}