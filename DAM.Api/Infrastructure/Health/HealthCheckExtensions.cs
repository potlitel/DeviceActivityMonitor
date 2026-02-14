using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Text.Json;

namespace DAM.Api.Infrastructure.Health;

public static class HealthCheckExtensions
{
    public static HealthCheckOptions GetJsonOptions()
    {
        return new HealthCheckOptions
        {
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";
                var response = new
                {
                    Status = report.Status.ToString(),
                    Duration = report.TotalDuration,
                    Checks = report.Entries.Select(e => new
                    {
                        Component = e.Key,
                        Status = e.Value.Status.ToString(),
                        Description = e.Value.Description,
                        Duration = e.Value.Duration
                    })
                };
                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
        };
    }
}