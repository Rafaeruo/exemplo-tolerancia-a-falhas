using Microsoft.AspNetCore.Mvc;
using ToleranciaFalhas.MessageBroker.Saga;
using ToleranciaFalhas.MessageBroker.Saga.Events;
using ToleranciaFallhas.Shared.Saga.OrderSaga;

namespace ToleranciaFalhas.MessageBroker.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrderEventsController : ControllerBase
    {
        readonly OrderSaga _orderSaga;

        public OrderEventsController(OrderSaga ordersaga)
        {
            _orderSaga = ordersaga;
        }

        [HttpPut]
        [Route("NewOrderEvent")]
        public async Task<IActionResult> PutNewOrderEvent([FromBody] OrderSagaStateDto exampleStateDto)
        {
            var state = new OrderSagaState(exampleStateDto);
            var exampleEvent = new NewOrderEvent(state);

            await _orderSaga.Update(exampleEvent);

            return Ok();
        }

        [HttpPut]
        [Route("PaymentApprovedEvent")]
        public async Task<IActionResult> PutPaymentApprovedEvent([FromBody] OrderSagaStateDto exampleStateDto)
        {
            var state = new OrderSagaState(exampleStateDto);
            var exampleEvent = new PaymentApprovedEvent(state);

            await _orderSaga.Update(exampleEvent);

            return Ok();
        }

        [HttpPut]
        [Route("PaymentRejectedEvent")]
        public async Task<IActionResult> PutPaymentRejectedEvent([FromBody] OrderSagaStateDto exampleStateDto)
        {
            var state = new OrderSagaState(exampleStateDto);
            var exampleEvent = new PaymentRejectedEvent(state);

            await _orderSaga.Update(exampleEvent);

            return Ok();
        }
    }
}