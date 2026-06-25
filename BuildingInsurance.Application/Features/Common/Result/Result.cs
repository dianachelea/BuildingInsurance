namespace BuildingInsurance.Application.Features.Common.Result
{
    public class Result
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public string Error { get; }
        public ErrorType ErrorType { get; }

        protected Result(bool isSuccess, string error, ErrorType errorType)
        {
            IsSuccess = isSuccess;
            Error = error;
            ErrorType = errorType;
        }

        public static Result Success() => new(true, string.Empty, ErrorType.None);
        
        public static Result Failure(string error, ErrorType errorType = ErrorType.Generic) => new(false, error, errorType);
        
        public static Result Conflict(string? error = null) =>
            new(false, "A concurrency conflict occured.", ErrorType.Conflict);
    }

    public class Result<T> : Result
    {
        public T Value { get; }

        protected Result(bool isSuccess, T value, string error, ErrorType errorType)
            : base(isSuccess, error, errorType)
        {
            Value = value;
        }

        public static Result<T> Success(T value) =>
            new(true, value, string.Empty, ErrorType.None);

        public static new Result<T> Failure(string error, ErrorType errorType) =>
            new(false, default!, error, errorType);

        public static new Result<T> Conflict(string? error = null)
            => new(false, default!, error ?? "A concurrency conflict occurred.", ErrorType.Conflict);

        public static implicit operator Result<T>(T value) =>
            Success(value);

        public static implicit operator Result<T>(string error) =>
            Failure(error, ErrorType.Generic);
    }
}