namespace EmployeeApi.Middleware
{
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var correlationId =
                Guid.NewGuid().ToString();

            context.Items["CorrelationId"] = correlationId;

            context.Response.Headers["X-Correlation-ID"] =
                correlationId;

            using (Serilog.Context.LogContext.PushProperty(
                "CorrelationId", correlationId))
            {
                await _next(context);
            }
        }
    }
}
