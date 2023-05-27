using System.Text.Json;

namespace learning_middlewares.Middleware
{
    public class TimingMiddleware
    {
        private readonly ILogger<TimingMiddleware> _logger;
        private readonly RequestDelegate _next;

        public TimingMiddleware(RequestDelegate next, ILogger<TimingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            var start = DateTime.UtcNow;
            await _next(context);

            _logger.LogInformation($"Timing: {(DateTime.UtcNow - start).TotalMilliseconds}ms");

            context.Response.StatusCode = 200;
            context.Response.ContentType = "application/json";

            var path = context.Request.Path;
            var method = context.Request.Method;
            var statusCode = context.Response.StatusCode;
            var responseTime = (DateTime.UtcNow - start).TotalMilliseconds;

            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                path,
                method,
                statusCode,
                responseTime
            }));

            _logger.LogInformation($"Request: {method} {path} responded {statusCode} in {responseTime} ms");
        }
    }

    public static class TimingMiddlewareExtensions
    {
        public static IApplicationBuilder UseTiming(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<TimingMiddleware>();
        }

        // public static void AddTiming(this IServiceCollection services)
        // {
        //     services.AddTransient<ITiming, SomeTiming>();
        // }
    }
}