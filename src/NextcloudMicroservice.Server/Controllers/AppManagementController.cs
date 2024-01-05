using System.Text;
using Microsoft.AspNetCore.Mvc;
using NextcloudMicroservice.Server.Services;

namespace NextcloudMicroservice.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class AppManagementController : ControllerBase
{
    private readonly ILogger<AppManagementController> _logger;
    private readonly IConfiguration _configuration;

    public AppManagementController(ILogger<AppManagementController> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    [HttpGet("/heartbeat")]
    public IActionResult GetHeartbeat() {
        _logger.LogInformation("Heartbeat");
        return Ok(new { status = "ok" });
    }

    [HttpPost("/init")]
    public IActionResult PostInit() {
        _logger.LogInformation("Init");
        return StatusCode(501);
    }

    [HttpPut("/enabled")]
    public async Task<IActionResult> PutEnabled([FromQuery(Name = "enabled")] int enabled) {
        var configSection = _configuration.GetSection("Nextcloud");
        var predefinedSecret = configSection.GetValue<string>("Secret");
        _logger.LogInformation("Secret from setting: {secret}", predefinedSecret);

        var encodedAuth = Request.Headers?["AUTHORIZATION-APP-API"].ToString() ?? throw new ArgumentException("Nextcloud header missing");
        byte[] data = Convert.FromBase64String(encodedAuth);
        string decodedString = Encoding.UTF8.GetString(data);
        var secret = decodedString.Split(":")[1];
        _logger.LogInformation("Secret from request: {secret}", secret);
        
        if (!secret.Equals(predefinedSecret))
        {
            return StatusCode(500, new { error = "The given secret is incorrect." });
        }

        var baseUrl = $"{configSection.GetValue<string>("Url")}/ocs/v1.php/apps/app_api/api/v1";
        var headers = new List<KeyValuePair<string, string>>
        {
            new("OCS-APIRequest", "true"),
            new("AA-VERSION", Request.Headers?["AA-VERSION"].ToString() ?? throw new ArgumentException("Nextcloud header missing")),
            new("EX-APP-ID", Request.Headers?["EX-APP-ID"].ToString() ?? throw new ArgumentException("Nextcloud header missing")),
            new("EX-APP-VERSION", Request.Headers?["EX-APP-VERSION"].ToString() ?? throw new ArgumentException("Nextcloud header missing")),
            new("AUTHORIZATION-APP-API", Request.Headers?["AUTHORIZATION-APP-API"].ToString() ?? throw new ArgumentException("Nextcloud header missing"))
        };
        using var nextcloudClient = new NextcloudClient(baseUrl, headers);

        if (enabled == 1)
        {
            _logger.LogInformation("Add the required stuff...");

            var resp1Content = await nextcloudClient.Post("ui/top-menu", new { name = "weather_forecast", displayName = ".NET Microservice" });
            _logger.LogInformation(resp1Content);

            var resp2Content = await nextcloudClient.Post("ui/script", new { type = "top_menu", name = "weather_forecast", path = "script" });
            _logger.LogInformation(resp2Content);

            var resp3Content = await nextcloudClient.Post("ui/style", new { type = "top_menu", name = "weather_forecast", path = "style" });
            _logger.LogInformation(resp3Content);
        }
        else
        {
            _logger.LogInformation("Remove the required stuff...");

            var resp1Content = await nextcloudClient.Delete("ui/top-menu", new { name = "weather_forecast" });
            _logger.LogInformation(resp1Content);

            var resp2Content = await nextcloudClient.Delete("ui/script", new { type = "top_menu", name = "weather_forecast", path = "script" });
            _logger.LogInformation(resp2Content);

            var resp3Content = await nextcloudClient.Delete("ui/style", new { type = "top_menu", name = "weather_forecast", path = "style" });
            _logger.LogInformation(resp3Content);
        }

        return Ok(new { error = string.Empty });
    }
}
