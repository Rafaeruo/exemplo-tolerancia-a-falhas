using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Linq;
using ToleranciaFalhas.MessageBroker;

namespace ToleranciaFalhas.MessageBroker.Controllers
{
    [ApiController]
    [Route("")]
    public class ProxyController : ControllerBase
    {
        private readonly CircuitBreakerManager _circuitBreakerManager;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ProxyController> _logger;
        private readonly GatewayConfig _gatewayConfig;

        public ProxyController(
            CircuitBreakerManager circuitBreakerManager,
            IHttpClientFactory httpClientFactory,
            ILogger<ProxyController> logger,
            IOptions<GatewayConfig> gatewayConfig)
        {
            _circuitBreakerManager = circuitBreakerManager;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _gatewayConfig = gatewayConfig.Value;
        }

        [HttpGet, HttpPost, HttpPut, HttpDelete, HttpPatch]
        [Route("{service}/{*path}")]
        public async Task<IActionResult> ProxyRequest(string service, string path)
        {
            var serviceConfig = _gatewayConfig.Services.FirstOrDefault(s => string.Equals(s.Name, service, StringComparison.OrdinalIgnoreCase));

            if (serviceConfig == null)
            {
                _logger.LogError($"Service '{service}' not found :shrug:.");
                return NotFound($"Service '{service}' not found.");
            }

            if (!_circuitBreakerManager.CanProceed(service))
            {
                return StatusCode(503, $"Service '{service}' is currently unavailable.");
            }

            _logger.LogError($"{serviceConfig.BaseUrl}/{path}");

            var matchedRoute = serviceConfig.Routes.Any(r => r.HttpVerb == Request.Method && r.Path == "/" + path);

            if (!matchedRoute)
            {
                _logger.LogError($"Unknown route {path} for service {service}.");
                return NotFound($"Unknown route {path} for service {service}.");
            }

            var httpRequestMessage = new HttpRequestMessage
            {
                Method = Request.Method switch
                {
                    "GET" => HttpMethod.Get,
                    "POST" => HttpMethod.Post,
                    "PUT" => HttpMethod.Put,
                    "DELETE" => HttpMethod.Delete,
                    "PATCH" => HttpMethod.Patch,
                    _ => HttpMethod.Get
                },
                RequestUri = new Uri($"{serviceConfig.BaseUrl}/{path}")
            };

            var client = _httpClientFactory.CreateClient();

            try 
            {
                var response = await client.SendAsync(httpRequestMessage);

                if ((int)response.StatusCode >= 500)
                {
                    _circuitBreakerManager.ReportFailure(service);
                }
                else
                {
                    _circuitBreakerManager.ReportSuccess(service);
                }

                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
            } 
            catch (Exception e) 
            {
                _circuitBreakerManager.ReportFailure(service);
                _logger.LogError(e, "It's so over :sob:");
                return StatusCode(500, e.Message);
            }

        }
    }
}
