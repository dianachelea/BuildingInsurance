using BuildingInsurance.Application.Features.Administrators.Reports.Common.Models;
using BuildingInsurance.Application.Features.Administrators.Reports.Queries.GetPolicyReport;
using BuildingInsurance.Application.Features.Common.Contracts.Enums;

namespace BuildingInsurance.Tests.Validators.PolicyReports
{
    public sealed class GetPolicyReportValidatorTests
    {
        [Fact]
        public void Should_Pass_For_Valid_Query()
        {
            var validator = new GetPolicyReportValidator();

            var query = new GetPolicyReportQuery(
                Dimension: ReportDimension.Country,
                Filters: new PolicyReportFilters(
                    From: new DateTime(2026, 02, 01, 0, 0, 0, DateTimeKind.Utc),
                    To: new DateTime(2026, 02, 28, 0, 0, 0, DateTimeKind.Utc),
                    Status: PolicyStatusContract.Active,
                    CurrencyCode: "EUR",
                    BuildingType: BuildingTypeContract.Residential));

            var result = validator.Validate(query);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Should_Fail_When_From_Is_Empty()
        {
            var validator = new GetPolicyReportValidator();

            var query = new GetPolicyReportQuery(
                Dimension: ReportDimension.Country,
                Filters: new PolicyReportFilters(
                    From: default,
                    To: new DateTime(2026, 02, 28, 0, 0, 0, DateTimeKind.Utc),
                    Status: PolicyStatusContract.Active,
                    CurrencyCode: "EUR",
                    BuildingType: null));

            var result = validator.Validate(query);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Filters.From");
        }

        [Fact]
        public void Should_Fail_When_To_Is_Empty()
        {
            var validator = new GetPolicyReportValidator();

            var query = new GetPolicyReportQuery(
                Dimension: ReportDimension.Country,
                Filters: new PolicyReportFilters(
                    From: new DateTime(2026, 02, 01, 0, 0, 0, DateTimeKind.Utc),
                    To: default,
                    Status: PolicyStatusContract.Active,
                    CurrencyCode: "EUR",
                    BuildingType: null));

            var result = validator.Validate(query);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Filters.To");
        }

        [Fact]
        public void Should_Fail_When_From_Is_After_To()
        {
            var validator = new GetPolicyReportValidator();

            var query = new GetPolicyReportQuery(
                Dimension: ReportDimension.Country,
                Filters: new PolicyReportFilters(
                    From: new DateTime(2026, 03, 01, 0, 0, 0, DateTimeKind.Utc),
                    To: new DateTime(2026, 02, 01, 0, 0, 0, DateTimeKind.Utc),
                    Status: PolicyStatusContract.Active,
                    CurrencyCode: "EUR",
                    BuildingType: null));

            var result = validator.Validate(query);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "" && e.ErrorMessage == "From must be earlier than or equal to To.");
        }

        [Fact]
        public void Should_Fail_When_Dimension_Is_Invalid()
        {
            var validator = new GetPolicyReportValidator();

            var query = new GetPolicyReportQuery(
                Dimension: (ReportDimension)999,
                Filters: new PolicyReportFilters(
                    From: new DateTime(2026, 02, 01, 0, 0, 0, DateTimeKind.Utc),
                    To: new DateTime(2026, 02, 28, 0, 0, 0, DateTimeKind.Utc),
                    Status: PolicyStatusContract.Active,
                    CurrencyCode: "EUR",
                    BuildingType: null));

            var result = validator.Validate(query);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Dimension" && e.ErrorMessage == "Report dimension is invalid.");
        }

        [Fact]
        public void Should_Fail_When_BuildingType_Is_Invalid_And_HasValue()
        {
            var validator = new GetPolicyReportValidator();

            var query = new GetPolicyReportQuery(
                Dimension: ReportDimension.City,
                Filters: new PolicyReportFilters(
                    From: new DateTime(2026, 02, 01, 0, 0, 0, DateTimeKind.Utc),
                    To: new DateTime(2026, 02, 28, 0, 0, 0, DateTimeKind.Utc),
                    Status: PolicyStatusContract.Active,
                    CurrencyCode: "EUR",
                    BuildingType: (BuildingTypeContract)999));

            var result = validator.Validate(query);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Filters.BuildingType" && e.ErrorMessage == "Building type is invalid.");
        }

        [Fact]
        public void Should_Fail_When_Status_Is_Invalid_And_HasValue()
        {
            var validator = new GetPolicyReportValidator();

            var query = new GetPolicyReportQuery(
                Dimension: ReportDimension.Broker,
                Filters: new PolicyReportFilters(
                    From: new DateTime(2026, 02, 01, 0, 0, 0, DateTimeKind.Utc),
                    To: new DateTime(2026, 02, 28, 0, 0, 0, DateTimeKind.Utc),
                    Status: PolicyStatusContract.Draft,
                    CurrencyCode: "EUR",
                    BuildingType: null));

            var result = validator.Validate(query);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Filters.Status" && e.ErrorMessage == "Policy status must be Active, Expired, or Cancelled.");
        }

        [Fact]
        public void Should_Fail_When_CurrencyCode_Is_Invalid_And_HasValue()
        {
            var validator = new GetPolicyReportValidator();

            var query = new GetPolicyReportQuery(
                Dimension: ReportDimension.Country,
                Filters: new PolicyReportFilters(
                    From: new DateTime(2026, 02, 01, 0, 0, 0, DateTimeKind.Utc),
                    To: new DateTime(2026, 02, 28, 0, 0, 0, DateTimeKind.Utc),
                    Status: PolicyStatusContract.Active,
                    CurrencyCode: "EURO",
                    BuildingType: null));

            var result = validator.Validate(query);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Filters.CurrencyCode" && e.ErrorMessage == "Currency code is invalid.");
        }

        [Fact]
        public void Should_Pass_When_Optional_Enums_Are_Null_BuildingTypeOnly()
        {
            var validator = new GetPolicyReportValidator();

            var query = new GetPolicyReportQuery(
                Dimension: ReportDimension.County,
                Filters: new PolicyReportFilters(
                    From: new DateTime(2026, 02, 01, 0, 0, 0, DateTimeKind.Utc),
                    To: new DateTime(2026, 02, 28, 0, 0, 0, DateTimeKind.Utc),
                    Status: PolicyStatusContract.Active,
                    CurrencyCode: "EUR",
                    BuildingType: null));

            var result = validator.Validate(query);

            Assert.True(result.IsValid);
        }
    }
}