using System.Text;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging(builder => builder.AddConsole());

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", (HttpRequest request) => {
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithOpenApi();

app.MapGet("/heartbeat", ([FromServices] ILogger<Program> logger) => {
    logger.LogInformation("Heartbeat");
    return Results.Json(new { status = "ok" }, statusCode: 200);
});

app.MapPost("/init", ([FromServices] ILogger<Program> logger) => {
    logger.LogInformation("Init");
    return Results.StatusCode(501);
});

app.MapPut("/enabled", async ([FromQuery(Name = "enabled")] int enabled, HttpRequest request, [FromServices] IConfiguration configuration, [FromServices] ILogger<Program> logger) => {
    var predefinedSecret = configuration.GetValue<string>("NextcloudSecret");
    logger.LogInformation("Secret from setting: {secret}", predefinedSecret);

    var encodedAuth = request.Headers?["AUTHORIZATION-APP-API"].ToString() ?? throw new ArgumentException("Nextcloud header missing");
    byte[] data = Convert.FromBase64String(encodedAuth);
    string decodedString = Encoding.UTF8.GetString(data);
    var secret = decodedString.Split(":")[1];
    logger.LogInformation("Secret from request: {secret}", secret);
    
    if (!secret.Equals(predefinedSecret))
    {
        return Results.Json(new { error = "The given secret is incorrect." }, statusCode: 500);
    }

    var baseUrl = $"{configuration.GetValue<string>("NextcloudUrl")}/ocs/v1.php/apps/app_api/api/v1";
    var headers = new List<KeyValuePair<string, string>>
    {
        new("OCS-APIRequest", "true"),
        new("AA-VERSION", request.Headers?["AA-VERSION"].ToString() ?? throw new ArgumentException("Nextcloud header missing")),
        new("EX-APP-ID", request.Headers?["EX-APP-ID"].ToString() ?? throw new ArgumentException("Nextcloud header missing")),
        new("EX-APP-VERSION", request.Headers?["EX-APP-VERSION"].ToString() ?? throw new ArgumentException("Nextcloud header missing")),
        new("AUTHORIZATION-APP-API", request.Headers?["AUTHORIZATION-APP-API"].ToString() ?? throw new ArgumentException("Nextcloud header missing"))
    };
    using var nextcloudClient = new NextcloudClient(baseUrl, headers);

    if (enabled == 1)
    {
        logger.LogInformation("Add the required stuff...");

        var resp1Content = await nextcloudClient.Post("ui/top-menu", new { name = "weather_forecast", displayName = ".NET Microservice" });
        logger.LogInformation(resp1Content);

        var resp2Content = await nextcloudClient.Post("ui/script", new { type = "top_menu", name = "weather_forecast", path = "script" });
        logger.LogInformation(resp2Content);

        var resp3Content = await nextcloudClient.Post("ui/style", new { type = "top_menu", name = "weather_forecast", path = "style" });
        logger.LogInformation(resp3Content);
    }
    else
    {
        logger.LogInformation("Remove the required stuff...");

        var resp1Content = await nextcloudClient.Delete("ui/top-menu", new { name = "weather_forecast" });
        logger.LogInformation(resp1Content);

        var resp2Content = await nextcloudClient.Delete("ui/script", new { type = "top_menu", name = "weather_forecast", path = "script" });
        logger.LogInformation(resp2Content);

        var resp3Content = await nextcloudClient.Delete("ui/style", new { type = "top_menu", name = "weather_forecast", path = "style" });
        logger.LogInformation(resp3Content);
    }

    return Results.Json(new { error = string.Empty }, statusCode: 200);
})
.WithOpenApi();

app.UseStaticFiles();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
