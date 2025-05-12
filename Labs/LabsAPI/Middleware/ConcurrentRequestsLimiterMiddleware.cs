using System.Text.Json;
using LabsAPI.Model;
using Microsoft.Extensions.Options;

public class ConcurrentRequestsLimiterMiddleware
{
    private readonly RequestDelegate _next;
    private readonly int _maxConcurrentRequests;
    private static int _currentRequests = 0;
    private static readonly SemaphoreSlim _semaphore = new(1, 1);

    public ConcurrentRequestsLimiterMiddleware(
        RequestDelegate next,
        IOptions<RateLimitingSettings> settings
    )
    {
        _next = next;
        _maxConcurrentRequests = settings.Value.MaxConcurrentRequests;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        bool requestAllowed = false;

        try
        {
            await _semaphore.WaitAsync();

            if (_currentRequests < _maxConcurrentRequests)
            {
                _currentRequests++;
                requestAllowed = true;
            }
        }
        finally
        {
            _semaphore.Release();
        }

        if (!requestAllowed)
        {
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(
                JsonSerializer.Serialize(
                    new
                    {
                        Error = "Service Unavailable",
                        Message = "Maximum concurrent requests limit reached",
                        RetryAfter = "30 seconds"
                    }
                )
            );
            return;
        }

        try
        {
            await _next(context);
        }
        finally
        {
            Interlocked.Decrement(ref _currentRequests);
        }
    }
}
