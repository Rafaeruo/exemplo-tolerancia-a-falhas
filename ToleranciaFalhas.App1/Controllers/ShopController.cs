using System.Threading.Tasks;
using System.Transactions;
using Microsoft.AspNetCore.Mvc;
using ToleranciaFalhas.App1.Database;
using ToleranciaFalhas.App1.Models;

namespace ToleranciaFalhas.App1.Controllers;

[ApiController]
[Route("[controller]")]
public class ShopController : ControllerBase
{
    private readonly ILogger<ShopController> _logger;

    private readonly IDatabase<Guid, Order> _database;

    private readonly IHttpClientFactory _httpClientFactory;

    public ShopController(ILogger<ShopController> logger, IHttpClientFactory httpClientFactory, IDatabase<Guid, Order> database)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
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

        var client = _httpClientFactory.CreateClient();

        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri("http://127.0.0.1:7000/Saga/")
        };

        var response = await client.SendAsync(request);

        TransactionDto? transaction = await response.Content.ReadFromJsonAsync<TransactionDto>();

        if (transaction == null)
        {
            return StatusCode(500, "Saga orchestrator failed to respond.");
        }

        Order order = new Order
        {
            Item = item.Item,
            PaymentStatus = transaction.Step
        };

        order.SetTransactionKey(transaction.Key);

        Guid orderNumber = _database.Save(order);

        _logger.LogInformation("Created new order with id: {}", orderNumber.ToString());

        Response.Headers.Append("location", "/Shop/" + orderNumber.ToString());

        return StatusCode(201, new OutOrderDto
        {
            Item = order.Item,
            PaymentStatus = order.PaymentStatus,
            OrderNumber = orderNumber,
        });
    }

    [HttpPut]
    public void Put([FromBody] OutOrderDto order)
    {
        _database.Update(order.OrderNumber, order);

        if (order.PaymentStatus == PaymentStatus.Paid)
        {
            _logger.LogInformation("Shipping to customer!");
        }
    }
}