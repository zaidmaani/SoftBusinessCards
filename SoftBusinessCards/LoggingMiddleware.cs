using Serilog.Context;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

public class LoggingMiddleware
{
    private readonly RequestDelegate _next;

    public LoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var ipAddress = context.Connection.RemoteIpAddress?.ToString();
        using (LogContext.PushProperty("IpAddress", ipAddress))
        {
            await _next(context);
        }
    }
}
