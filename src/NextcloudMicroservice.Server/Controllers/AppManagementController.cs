using Microsoft.AspNetCore.Mvc;
using NextcloudMicroservice.Server.Attributes;
using NextcloudMicroservice.Server.Clients;

namespace NextcloudMicroservice.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class AppManagementController(ILogger<AppManagementController> logger, NextcloudClient nextcloudClient) : ControllerBase
{
    private readonly ILogger<AppManagementController> _logger = logger;
    private readonly NextcloudClient _nextcloudClient = nextcloudClient;

    [HttpGet("/heartbeat")]
    public IActionResult GetHeartbeat() {
        _logger.LogInformation("Heartbeat");
        return Ok(new { status = "ok" });
    }

    [HttpPost("/init")]
    [TypeFilter(typeof(SharedSecretHeaderFilter))]
    public IActionResult PostInit() {
        _logger.LogInformation("Init");
        return StatusCode(501);
    }

    [HttpPut("/enabled")]
    [TypeFilter(typeof(SharedSecretHeaderFilter))]
    public async Task<IActionResult> PutEnabled([FromQuery(Name = "enabled")] int enabled) {
        _nextcloudClient.SetDefaultRequestHeaders(Request.Headers);

        if (enabled == 1)
        {
            _logger.LogInformation("Add the required stuff...");

            var resp1Content = await _nextcloudClient.Post("ui/top-menu", new { name = "weather_forecast", displayName = ".NET Microservice" });
            _logger.LogInformation(resp1Content);

            var resp2Content = await _nextcloudClient.Post("ui/script", new { type = "top_menu", name = "weather_forecast", path = "script" });
            _logger.LogInformation(resp2Content);

            var resp3Content = await _nextcloudClient.Post("ui/style", new { type = "top_menu", name = "weather_forecast", path = "style" });
            _logger.LogInformation(resp3Content);
        }
        else
        {
            _logger.LogInformation("Remove the required stuff...");

            var resp1Content = await _nextcloudClient.Delete("ui/top-menu", new { name = "weather_forecast" });
            _logger.LogInformation(resp1Content);

            var resp2Content = await _nextcloudClient.Delete("ui/script", new { type = "top_menu", name = "weather_forecast", path = "script" });
            _logger.LogInformation(resp2Content);

            var resp3Content = await _nextcloudClient.Delete("ui/style", new { type = "top_menu", name = "weather_forecast", path = "style" });
            _logger.LogInformation(resp3Content);
        }

        return Ok(new { error = string.Empty });
    }
}
