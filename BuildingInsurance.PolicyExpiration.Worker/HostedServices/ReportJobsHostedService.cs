using BuildingInsurance.Application.Features.Common.Abstractions;

namespace BuildingInsurance.Policy.Worker.HostedServices
{
    public sealed class ReportJobsHostedService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IClock _clock;
        private readonly ILogger<ReportJobsHostedService> _logger;

        public ReportJobsHostedService(IServiceScopeFactory scopeFactory, IClock clock, ILogger<ReportJobsHostedService> logger)
        {
            _scopeFactory = scopeFactory;
            _clock = clock;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ReportJobsHostedService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var runner = scope.ServiceProvider.GetRequiredService<IReportJobsRunner>();

                    await runner.RunOnceAsync(_clock.UtcNow, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("ReportJobsHostedService is stopping.");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while processing report jobs.");
                }

                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("ReportJobsHostedService is stopping.");
                    break;
                }
            }
        }
    }
}