namespace BuildingInsurance.Application.Features.Common.Result
{
    public enum ErrorType
    {
        None = 0,
        Generic,
        Validation,
        NotFound,
        Conflict,
        Unauthorized,
        Forbidden,
        BusinessRule
    }
}