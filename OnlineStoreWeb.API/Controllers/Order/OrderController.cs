using Microsoft.AspNetCore.Mvc;

namespace OnlineStoreWeb.API.Controllers.Order
{
    [ApiController]
    [Route("orders")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<OrderController> _logger;

        public OrderController(IOrderRepository orderRepository, ILogger<OrderController> logger)
        {
            _orderRepository = orderRepository;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder(CreateOrderDto orderDto)
        {
            try
            {
                if (orderDto == null)
                    return BadRequest(new { message = "Order data is required" });

                await _orderRepository.AddOrderAsync(orderDto);
                return Created($"orders", new { message = "Order created successfully" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Validation error while creating order: {Message}", ex.Message);
                return BadRequest(new { message = "Invalid order data provided" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating order: {Message}", ex.Message);
                return StatusCode(500, "An unexpected error occurred while creating the order");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            try
            {
                var orders = await _orderRepository.GetAllOrdersAsync();
                if (!orders.Any())
                    return NotFound(new { message = "No orders found" });

                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while fetching orders: {Message}", ex.Message);
                return StatusCode(500, "An unexpected error occurred while fetching orders");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderWithId(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "Invalid order ID" });

                var order = await _orderRepository.GetOrderWithIdAsync(id);
                if (order == null)
                    return NotFound(new { message = $"Order with ID {id} not found" });

                return Ok(order);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Order fetching process error: {Message}", ex.Message);
                return BadRequest(new { message = "Order fetching process error" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while fetching order: {Message}", ex.Message);
                return StatusCode(500, "An unexpected error occurred while fetching the order");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder(int id, UpdateOrderDto updateDto)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "Invalid order ID" });

                if (updateDto == null)
                    return BadRequest(new { message = "Order update data is required" });

                await _orderRepository.UpdateOrderAsync(id, updateDto);
                return Ok(new { message = "Order updated successfully" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Validation error while updating order: {Message}", ex.Message);
                return BadRequest(new { message = "Invalid order data provided" });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Order not found for update: {Message}", ex.Message);
                return NotFound(new { message = $"Order with ID {id} not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating order: {Message}", ex.Message);
                return StatusCode(500, "An unexpected error occurred while updating the order");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "Invalid order ID" });

                await _orderRepository.DeleteOrderAsync(id);
                return Ok(new { message = "Order deleted successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Order not found for deletion: {Message}", ex.Message);
                return NotFound(new { message = $"Order with ID {id} not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while deleting order: {Message}", ex.Message);
                return StatusCode(500, "An unexpected error occurred while deleting the order");
            }
        }
    }
}