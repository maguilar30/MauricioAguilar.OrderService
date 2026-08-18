using MauricioAguilar.OrderService.DTOs;
using MauricioAguilar.OrderService.Services;
using Microsoft.AspNetCore.Mvc;

namespace MauricioAguilar.OrderService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private IOrderService _service;
        private readonly ILogger<OrderController> _logger;

        public OrderController(IOrderService service, ILogger<OrderController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Gets a list of Orders.
        /// </summary>
        /// <returns>A list of Orders.</returns>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(await _service.GetOrders());
        }

        /// <summary>
        /// Gets an Order by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier.</param>
        /// <returns></returns>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetByIdAsync([FromRoute] int id)
        {
            _logger.LogInformation("Getting order {OrderId}", id);
            return Ok(await _service.GetById(id));
        }

        /// <summary>
        /// Add a new Order.
        /// </summary>
        /// <param name="request">The new Order information.</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOrderRequest request, CancellationToken cancellationToken)
        {
            _logger.LogWarning("warning log {Customerid}", request.CustomerId);
            var order = await _service.CreateOrder(request, cancellationToken);
            //_logger.LogInformation("Creating order {OrderId}", order.Id);
            return CreatedAtAction(nameof(Get), new { id = order.Id }, order);
        }

    }
}
