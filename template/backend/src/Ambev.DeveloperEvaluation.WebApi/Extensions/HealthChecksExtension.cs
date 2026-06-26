using System.Net.Mime;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Ambev.DeveloperEvaluation.WebApi.Extensions;

/// <summary>
/// Provides extension methods for configuring basic liveness/readiness health checks.
/// </summary>
public static class HealthChecksExtension
{
    public static void AddBasicHealthChecks(this WebApplicationBuilder builder)
    {
        builder.Services.AddHealthChecks()
            .AddCheck("Liveness", () => HealthCheckResult.Healthy(), tags: ["liveness"])
            .AddCheck("Readiness", () => HealthCheckResult.Healthy(), tags: ["readiness"]);
    }

    public static void UseBasicHealthChecks(this WebApplication app)
    {
        app.UseHealthChecks("/health/live", WriteHealthCheckResponse(app, "liveness"));
        app.UseHealthChecks("/health/ready", WriteHealthCheckResponse(app, "readiness"));
        app.UseHealthChecks("/health", WriteHealthCheckResponse(app, string.Empty));

        var logger = app.Services.GetRequiredService<ILogger<HealthCheckService>>();
        logger.LogInformation("Health Check enabled at: '/health'");
    }

    private static HealthCheckOptions WriteHealthCheckResponse(this WebApplication app, string tag)
    {
        return new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains(tag),
            ResultStatusCodes =
            {
                [HealthStatus.Healthy] = StatusCodes.Status200OK,
                [HealthStatus.Degraded] = StatusCodes.Status200OK,
                [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
            },
            ResponseWriter = async (context, report) =>
            {
                var result = new
                {
                    status = report.Status.ToString(),
                    healthChecks = report.Entries.Select(e => new
                    {
                        name = e.Key,
                        status = e.Value.Status.ToString(),
                        description = e.Value.Description,
                        errorMessage = e.Value.Exception?.Message,
                        hostEnvironment = app.Environment.EnvironmentName.ToLowerInvariant()
                    }),
                };
                context.Response.ContentType = MediaTypeNames.Application.Json;
                await context.Response.WriteAsJsonAsync(result);
            },
        };
    }
}
