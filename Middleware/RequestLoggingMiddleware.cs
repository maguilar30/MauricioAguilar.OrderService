namespace MauricioAguilar.OrderService.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(
            RequestDelegate next,
            ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var startedAt = DateTime.UtcNow;

            

            _logger.LogInformation(
                "Request started: {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            await _next(context);

            var elapsed = DateTime.UtcNow - startedAt;

            if (context.Response.StatusCode >= 400)
            {
                _logger.LogWarning(
                "Request completed with client/server error: {StatusCode} in {ElapsedMilliseconds} ms",
                context.Response.StatusCode,
                elapsed.TotalMilliseconds);
            }
            else
            {
                _logger.LogInformation(
                "Request completed: {StatusCode} in {ElapsedMilliseconds} ms",
                context.Response.StatusCode,
                elapsed.TotalMilliseconds);
            }
        }
    }
}
