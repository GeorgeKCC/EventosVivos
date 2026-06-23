using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Transversal.Exceptions.Customs;
using Transversal.Exceptions.Model;

namespace Transversal.Exceptions.Handler
{
    internal class ExceptionHandler(ILogger<ExceptionHandler> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            logger.LogError("Error Message: {exceptionMessage}, Time of occurrence {time}", exception.Message, DateTime.UtcNow);
            (string Detail, string Title, int StatusCode, string ErrorCode) = exception switch
            {
                NotFoundCustomException =>
                (
                    exception.Message,
                    exception.GetType().Name,
                    httpContext.Response.StatusCode = StatusCodes.Status404NotFound,
                    "404"
                ),
                    VenueCapacityExceedsException => (
                    exception.Message,
                    exception.GetType().Name,
                    httpContext.Response.StatusCode = StatusCodes.Status400BadRequest,
                    "400"
                ),
                _ =>
                (
                   exception.Message,
                   exception.GetType().Name,
                   httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError,
                   "500"
                )
            };

            var problemDetails = new ProblemDetails
            {
                Title = Title,
                Detail = Detail,
                Status = StatusCode,
                Instance = httpContext.Request.Path
            };

            problemDetails.Extensions.Add("traceId", httpContext.TraceIdentifier);

            if (exception is ValidationException validationException)
            {
                problemDetails.Extensions.Add("ValidationErrors", validationException.Errors);
            }

            var response = new ResponseError<ProblemDetails>(ErrorCode, Detail, problemDetails);

            logger.LogError("Error Message: {message}", JsonSerializer.Serialize(response));

            await httpContext.Response.WriteAsJsonAsync(response, cancellationToken: cancellationToken);
            return true;
        }
    }
}
