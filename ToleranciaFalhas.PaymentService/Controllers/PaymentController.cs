using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ToleranciaFallhas.Shared.Saga.OrderSaga;

namespace ToleranciaFalhas.PaymentService.Controllers;

[ApiController]
[Route("[controller]")]
public class PaymentController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ProxyConfig _proxyConfig;

    public PaymentController(
        IHttpClientFactory httpClientFactory,
        IOptions<ProxyConfig> proxyConfig)
    {
        _httpClientFactory = httpClientFactory;
        _proxyConfig = proxyConfig.Value;
    }

    [HttpPost]
    public async Task<IActionResult> ProcessOrderPayment([FromBody] OrderSagaStateDto order)
    {
        var segundosDelay = Random.Shared.Next(3, 10);
        await Task.Delay(TimeSpan.FromSeconds(segundosDelay));

        var sucess = Random.Shared.Next(2) == 0;
        var eventName = sucess ? "PaymentApprovedEvent" : "PaymentRejectedEvent";

        var client = _httpClientFactory.CreateClient();

        await client.PutAsJsonAsync(_proxyConfig.BaseUrl + "/OrderEvents/" + eventName, order);

        return Ok();
    }
}
