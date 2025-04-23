using Microsoft.AspNetCore.Mvc;
using ToleranciaFalhas.App1.Saga;

namespace ToleranciaFalhas.MessageBroker.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SagaController : ControllerBase
    {
        readonly ExampleSaga _exampleSaga;

        public SagaController(IServiceScopeFactory serviceScopeFactory)
        {
            _exampleSaga = new ExampleSaga(serviceScopeFactory);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateSaga([FromBody] ExampleStateDto exampleStateDto)
        {
            ExampleState exampleState = new(exampleStateDto);
            ExampleEvent exampleEvent = new ExampleEvent(exampleState);
            await _exampleSaga.Update(exampleEvent);
            return StatusCode(200);
        }

        [HttpPost]
        public async Task<IActionResult> CreateSaga()
        {
            ExampleState exampleState = new ExampleState();
            ExampleEvent exampleEvent = new ExampleEvent(exampleState);
            await _exampleSaga.Update(exampleEvent);
            Console.WriteLine(exampleEvent.GetKey());
            return StatusCode(201, exampleState.ToDto());
        }
    }
}