using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace LMS.Services
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);
            _logger.LogError("Exception Type: {ExceptionType}", exception.GetType().Name);
            _logger.LogError("Stack Trace: {StackTrace}", exception.StackTrace);

            // For Blazor Server, we need to handle circuit exceptions differently
            if (httpContext.Request.Path.StartsWithSegments("/_blazor"))
            {
                _logger.LogError("Blazor circuit exception occurred");
                return false; // Let Blazor handle it
            }

            var response = new
            {
                title = "An error occurred",
                status = (int)HttpStatusCode.InternalServerError,
                detail = exception.Message,
                type = exception.GetType().Name
            };

            httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            httpContext.Response.ContentType = "application/json";

            var jsonResponse = JsonSerializer.Serialize(response);
            await httpContext.Response.WriteAsync(jsonResponse, cancellationToken);

            return true;
        }

       
    }
}
