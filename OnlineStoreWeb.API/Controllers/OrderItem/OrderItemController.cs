using Microsoft.AspNetCore.Mvc;

namespace OnlineStoreWeb.API.Controllers.OrderItem
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderItemController : ControllerBase
    {
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly ILogger<OrderItemController> _logger;

        public OrderItemController(IOrderItemRepository orderItemRepository, ILogger<OrderItemController> logger)
        {
            _orderItemRepository = orderItemRepository;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrderItem(CreateOrderItemDto orderItemDto)
        {
            try
            {
                if (orderItemDto == null)
                    return BadRequest(new { message = "Order item data is required" });

                await _orderItemRepository.AddOrderItemAsync(orderItemDto);
                return Created($"/api/orderitem", new { message = "Order item created successfully" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Validation error while creating order item: {Message}", ex.Message);
                return BadRequest(new { message = "Invalid order item data provided" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating order item: {Message}", ex.Message);
                return StatusCode(500, "An unexpected error occurred while creating the order item");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllOrderItems()
        {
            try
            {
                var orderItems = await _orderItemRepository.GetAllOrderItemsAsync();
                if (!orderItems.Any())
                    return NotFound(new { message = "No order items found" });

                return Ok(orderItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while fetching order items: {Message}", ex.Message);
                return StatusCode(500, "An unexpected error occurred while fetching order items");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderItemWithId(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "Invalid order item ID" });

                var orderItem = await _orderItemRepository.GetOrderItemWithIdAsync(id);
                if (orderItem == null)
                    return NotFound(new { message = $"Order item with ID {id} not found" });

                return Ok(orderItem);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Order item fetching process error: {Message}", ex.Message);
                return BadRequest(new { message = "Order item fetching process error" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while fetching order item: {Message}", ex.Message);
                return StatusCode(500, "An unexpected error occurred while fetching the order item");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrderItem(int id, UpdateOrderItemDto updateDto)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "Invalid order item ID" });

                if (updateDto == null)
                    return BadRequest(new { message = "Order item update data is required" });

                await _orderItemRepository.UpdateOrderItemAsync(updateDto);
                return Ok(new { message = "Order item updated successfully" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Validation error while updating order item: {Message}", ex.Message);
                return BadRequest(new { message = "Invalid order item data provided" });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Order item not found for update: {Message}", ex.Message);
                return NotFound(new { message = $"Order item with ID {id} not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating order item: {Message}", ex.Message);
                return StatusCode(500, "An unexpected error occurred while updating the order item");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrderItem(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "Invalid order item ID" });

                await _orderItemRepository.DeleteOrderItemAsync(id);
                return Ok(new { message = "Order item deleted successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Order item not found for deletion: {Message}", ex.Message);
                return NotFound(new { message = $"Order item with ID {id} not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while deleting order item: {Message}", ex.Message);
                return StatusCode(500, "An unexpected error occurred while deleting the order item");
            }
        }
    }
} 