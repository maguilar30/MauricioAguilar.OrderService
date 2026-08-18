using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace MauricioAguilar.OrderService.Exceptions
{
    public sealed class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            _logger.LogError(
            exception,
            "Unhandled exception occurred. TraceId: {TraceId}",
            httpContext.TraceIdentifier);

            var (statusCode, title) = exception switch
            {
                KeyNotFoundException => (
                    StatusCodes.Status404NotFound,
                    "Resource not found"),

                ArgumentException => (
                    StatusCodes.Status400BadRequest,
                    "Invalid request"),

                UnauthorizedAccessException => (
                    StatusCodes.Status403Forbidden,
                    "Access denied"),

                _ => (
                    StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred")
            };

            var problem = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = statusCode == 500
                    ? "Please contact support if the problem continues."
                    : exception.Message,
                Instance = httpContext.Request.Path
            };

            problem.Extensions["traceId"] = httpContext.TraceIdentifier;

            httpContext.Response.StatusCode = statusCode;

            await httpContext.Response.WriteAsJsonAsync(
                problem,
                cancellationToken);

            return true;
        }
    }
}
