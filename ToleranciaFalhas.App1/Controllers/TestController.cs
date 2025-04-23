using Microsoft.AspNetCore.Mvc;

namespace ToleranciaFalhas.OrderService.Controllers;

[ApiController]
[Route("[controller]")]
public class TestController : ControllerBase
{
    [HttpGet]
    [Route("failure")]
    public IActionResult FailureTest(Guid key)
    {
        return StatusCode(500);
    }
}