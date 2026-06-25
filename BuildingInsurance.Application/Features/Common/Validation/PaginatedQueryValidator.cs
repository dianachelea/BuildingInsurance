using BuildingInsurance.Application.Features.Common.Requests;
using FluentValidation;

namespace BuildingInsurance.Application.Features.Common.Validation
{
    public abstract class PaginatedQueryValidator<T> : AbstractValidator<T> where T : PaginatedQuery
    {
        protected PaginatedQueryValidator()
        {
            RuleFor(x => x.Page)
                .GreaterThan(0);

            RuleFor(x => x.PageSize)
                .GreaterThan(0)
                .LessThanOrEqualTo(100);
        }
    }
}