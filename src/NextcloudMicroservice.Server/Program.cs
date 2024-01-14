using NextcloudMicroservice.Server.Clients;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

var resourceBuilder = ResourceBuilder.CreateDefault().AddService(builder.Environment.ApplicationName);
var otlpEndpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];

builder.Services.AddLogging(builder =>
{
    if (!string.IsNullOrWhiteSpace(otlpEndpoint))
    {
        builder.AddOpenTelemetry(logging =>
                {
                    logging.SetResourceBuilder(resourceBuilder)
                            .AddOtlpExporter();
                });
    }
    else
    {
        builder.AddConsole();
    }
});

builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics =>
    {
        metrics.SetResourceBuilder(resourceBuilder)
                .AddAspNetCoreInstrumentation()
                .AddRuntimeInstrumentation()
                .AddHttpClientInstrumentation()
                .AddEventCountersInstrumentation(c =>
                {
                    // https://learn.microsoft.com/en-us/dotnet/core/diagnostics/available-counters
                    c.AddEventSources(
                        "Microsoft.AspNetCore.Hosting",
                        "Microsoft-AspNetCore-Server-Kestrel",
                        "System.Net.Http",
                        "System.Net.Sockets",
                        "System.Net.NameResolution",
                        "System.Net.Security");
                });

        if (!string.IsNullOrWhiteSpace(otlpEndpoint))
        {
            metrics.AddOtlpExporter();
        }
    })
    .WithTracing(tracing =>
    {
        tracing.SetResourceBuilder(resourceBuilder)
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation();

        if (!string.IsNullOrWhiteSpace(otlpEndpoint))
        {
            tracing.AddOtlpExporter();
        }
    });

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var nextcloudUrl = builder.Configuration.GetSection("Nextcloud")
                                        .GetValue<string>("Url") ?? throw new InvalidOperationException("Nextcloud URL is not configured");

builder.Services.AddHttpClient<NextcloudClient>(client =>
{
    client.BaseAddress = new(nextcloudUrl + "/ocs/v1.php/apps/app_api/api/v1");
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();
