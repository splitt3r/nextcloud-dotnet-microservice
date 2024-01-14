using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace NextcloudMicroservice.Server.Attributes;

public class SharedSecretHeaderFilter(ILogger<SharedSecretHeaderFilter> logger, IConfiguration configuration) : ActionFilterAttribute, IActionFilter
{
    private readonly ILogger<SharedSecretHeaderFilter> _logger = logger;
    private readonly string _secret = configuration.GetSection("Nextcloud")?.GetValue<string>("Secret") ?? throw new ArgumentException("The preshared secret (from config) can not be null");

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        try
        {
            // TODO: simplify this code
            if(!context.HttpContext.Request.Headers.TryGetValue("AUTHORIZATION-APP-API", out var encodedAuth))
            {
                context.Result = new UnauthorizedObjectResult("The AUTHORIZATION-APP-API header is missing");
                return;
            }

            byte[] data = Convert.FromBase64String(encodedAuth.ToString());
            string decodedString = Encoding.UTF8.GetString(data);
            var decodedStringSplit = decodedString.Split(":");

            if (decodedStringSplit.Length > 1 && !decodedStringSplit[1].Equals(_secret))
            {
                context.Result = new UnauthorizedObjectResult("The AUTHORIZATION-APP-API header is incorrect");
                return;
            }

            base.OnActionExecuting(context);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error while checking shared secret: {ex.Message}");
            context.Result = new UnauthorizedObjectResult("There was a problem parsing the AUTHORIZATION-APP-API header");
        }
    }
}
