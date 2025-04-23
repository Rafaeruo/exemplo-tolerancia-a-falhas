using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ToleranciaFalhas.OrderService.Database;
using ToleranciaFalhas.OrderService.Models;
using ToleranciaFallhas.Shared.Saga.OrderSaga;

namespace ToleranciaFalhas.OrderService.Controllers;

[ApiController]
[Route("[controller]")]
public class ShopController : ControllerBase
{
    private readonly ILogger<ShopController> _logger;

    private readonly IDatabase<Guid, Order> _database;

    private readonly IHttpClientFactory _httpClientFactory;

    private readonly ProxyConfig _proxyConfig;

    public ShopController(
        ILogger<ShopController> logger, 
        IHttpClientFactory httpClientFactory,
        IOptions<ProxyConfig> proxyConfig,
        IDatabase<Guid, Order> database)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _proxyConfig = proxyConfig.Value;
        _database = database;
    }

    [HttpGet]
    [Route("{key}")]
    public Order GetOrder(Guid key)
    {
        _logger.LogInformation("Getting order with id {}", key.ToString());
        return _database.Get(key);
    }

    [HttpPost]
    public async Task<IActionResult> PostOrder([FromBody] InOrderDto item)
    {
        var order = new Order
        {
            Item = item.Item,
            PaymentStatus = PaymentStatus.Pending
        };

        order.Id = _database.Save(order);

        var client = _httpClientFactory.CreateClient();
        var content = new OrderSagaStateDto(order.PaymentStatus, order.Id);
        var response = await client.PutAsJsonAsync(_proxyConfig.BaseUrl + "/OrderEvents/NewOrderEvent", content);

        Response.Headers.Append("location", "/Shop/" + order.Id.ToString());

        return StatusCode(201, order);
    }

    [HttpPatch]
    [Route("paymentApproved/{orderId}")]
    public void ConfirmPayment(Guid orderId)
    {
        var order = _database.Get(orderId);
        order.PaymentStatus = PaymentStatus.Paid;
        _database.Update(orderId, order);
    }

    [HttpPatch]
    [Route("paymentRejected/{orderId}")]
    public void RejectPayment(Guid orderId)
    {
        var order = _database.Get(orderId);
        order.PaymentStatus = PaymentStatus.PaymentRejected;
        _database.Update(orderId, order);
    }
}