using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRateLimiter(config =>
{

    config.OnRejected = (context, _) =>
    {
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter =
                ((int)retryAfter.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo);
        }

        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.WriteAsync("Too many requests. Please try again later.");

        return new ValueTask();
    };

    config.AddFixedWindowLimiter(policyName: "fixed", options =>
    {
        options.PermitLimit = 2;
        options.Window = TimeSpan.FromSeconds(15);
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 0;
    });
});

builder.Services.AddControllers();

var app = builder.Build();

app.UseRateLimiter();

app.MapGet("/api/resource", () =>
{
    var random = new Random();

    return Results.Ok(random.Next(1, 101));
}).RequireRateLimiting("fixed");

// No Limit
app.MapGet("/api/resource2", () =>
{
    var random = new Random();

    return Results.Ok(random.Next(1, 101));
}).DisableRateLimiting();

app.MapControllers();

app.Run();
