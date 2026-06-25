using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Common.Abstractions;
using BuildingInsurance.Application.Features.Common.Result;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BuildingInsurance.Application.Features.Administrators.Reports.Jobs.Commands.CreateReportJob
{
    public sealed class CreateReportJobCommandHandler : IRequestHandler<CreateReportJobCommand, Result<Guid>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IReportJobsRepository _reportJobsRepository;
        private readonly ILogger<CreateReportJobCommandHandler> _logger;
        private readonly IClock _clock;

        public CreateReportJobCommandHandler(IUnitOfWork unitOfWork, IClock clock, IReportJobsRepository reportJobsRepository,ILogger<CreateReportJobCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _reportJobsRepository = reportJobsRepository;
            _logger = logger;
            _clock = clock;
        }

        public async Task<Result<Guid>> Handle(CreateReportJobCommand request, CancellationToken cancellationToken)
        {
            bool transactionStarted = false;
            bool committed = false;

            try
            {
                await _unitOfWork.BeginTransactionAsync(cancellationToken);
                transactionStarted = true;

                var jobId = await _reportJobsRepository.CreateQueuedAsync(
                    dimension: request.Dimension,
                    filters: request.Filters,
                    nowUtc: _clock.UtcNow,
                    ct: cancellationToken);

                await _unitOfWork.CommitAsync(cancellationToken);
                committed = true;

                _logger.LogInformation("Report job created successfully. JobId={JobId}, Dimension={Dimension}", jobId, request.Dimension);

                return Result<Guid>.Success(jobId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during report job creation. Dimension={Dimension}", request.Dimension);

                return Result<Guid>.Failure(error: "Unexpected error during report job creation.", ErrorType.Generic);
            }
            finally
            {
                if (transactionStarted && !committed)
                {
                    try
                    {
                        await _unitOfWork.RollbackAsync(cancellationToken);
                    }
                    catch (Exception rbEx)
                    {
                        _logger.LogWarning(rbEx, "Final rollback attempt failed during report job creation. Dimension={Dimension}", request.Dimension);
                    }
                }
            }
        }
    }
}