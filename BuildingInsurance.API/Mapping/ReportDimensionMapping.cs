using BuildingInsurance.API.Contracts.Administrators.Reports;
using BuildingInsurance.Application.Features.Administrators.Reports.Common.Models;

namespace BuildingInsurance.API.Mapping
{
    public static class ReportDimensionMapping
    {
        public static ReportDimension MapToApplicationReportDimension(this ReportDimensionRequestDto type)
        {
            if (type == ReportDimensionRequestDto.City)
                return ReportDimension.City;
            else if (type == ReportDimensionRequestDto.County)
                return ReportDimension.County;
            else if (type == ReportDimensionRequestDto.Country)
                return ReportDimension.Country;
            else
                throw new ArgumentOutOfRangeException(nameof(type), $"Not expected report dimension type value: {type}");
        }
    }
}