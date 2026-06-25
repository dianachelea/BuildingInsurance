using BuildingInsurance.Application.Features.Common.Result;
using FluentValidation;
using System.Net;
using System.Text.Json;

namespace BuildingInsurance.API.Middlewares
{
    public static class ExceptionHandlingMiddleware
    {
        public static IApplicationBuilder UseResultExceptionHandling(this IApplicationBuilder app)
        {
            return app.Use(async (context, next) =>
            {
                try
                {
                    await next();
                }
                catch (ValidationException ex)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.ContentType = "application/json";

                    var errorMessage = string.Join("; ", ex.Errors.Select(e => e.ErrorMessage));

                    var result = Result.Failure(
                        error: errorMessage,
                        errorType: ErrorType.Validation
                    );

                    await context.Response.WriteAsync(JsonSerializer.Serialize(result));
                }
                catch (Exception ex)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.ContentType = "application/json";

                    var result = Result.Failure(
                        error: ex.Message,
                        errorType: ErrorType.Generic
                    );

                    await context.Response.WriteAsync(JsonSerializer.Serialize(result));
                }
            });
        }
    }
}