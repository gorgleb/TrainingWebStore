using Microsoft.AspNetCore.Mvc;
using TrainingWebStore.API.DTOs;
using TrainingWebStore.Core.Enums;
using TrainingWebStore.Core.Models;
using TrainingWebStore.Core.Services;

namespace TrainingWebStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase 
    {
        private readonly OrderService _orderService;

        public OrdersController(OrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDto>> GetOrder(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            return new OrderDto
            {
                Id = order.Id,
                OrderDate = order.OrderDate,
                CustomerId = order.CustomerId,
                CustomerName = $"{order.Customer?.FirstName} {order.Customer?.LastName}",
                Status = order.Status.ToString(),
                TotalAmount = order.TotalAmount,
                OrderItems = order.OrderItems.Select(oi => new OrderItemDto
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.Product?.Name,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice
                }).ToList()
            };
        }

        [HttpGet("customer/{customerId}")]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrdersByCustomer(int customerId)
        {
            var orders = await _orderService.GetOrdersByCustomerAsync(customerId);
            return Ok(orders.Select(o => new OrderDto
            {
                Id = o.Id,
                OrderDate = o.OrderDate,
                CustomerId = o.CustomerId,
                CustomerName = $"{o.Customer?.FirstName} {o.Customer?.LastName}",
                Status = o.Status.ToString(),
                TotalAmount = o.TotalAmount
            }));
        }

        [HttpPost]
        public async Task<ActionResult<OrderDto>> CreateOrder(CreateOrderDto createOrderDto)
        {
            try
            {
                var order = new Order
                {
                    CustomerId = createOrderDto.CustomerId,
                    OrderItems = createOrderDto.OrderItems.Select(oi => new OrderItem
                    {
                        ProductId = oi.ProductId,
                        Quantity = oi.Quantity
                    }).ToList()
                };

                var createdOrder = await _orderService.CreateOrderAsync(order);

                return CreatedAtAction(
                    nameof(GetOrder),
                    new { id = createdOrder.Id },
                    new OrderDto
                    {
                        Id = createdOrder.Id,
                        OrderDate = createdOrder.OrderDate,
                        CustomerId = createdOrder.CustomerId,
                        Status = createdOrder.Status.ToString(),
                        TotalAmount = createdOrder.TotalAmount,
                        OrderItems = createdOrder.OrderItems.Select(oi => new OrderItemDto
                        {
                            ProductId = oi.ProductId,
                            ProductName = oi.Product?.Name,
                            Quantity = oi.Quantity,
                            UnitPrice = oi.UnitPrice
                        }).ToList()
                    });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] string status)
        {
            try
            {
                if (!Enum.TryParse<OrderStatus>(status, out var orderStatus))
                {
                    return BadRequest("Invalid order status");
                }

                await _orderService.UpdateOrderStatusAsync(id, orderStatus);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelOrder(int id)
        {
            try
            {
                await _orderService.CancelOrderAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("customers/{customerId}/discount-info")]
        [ProducesResponseType(typeof(DiscountInfoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DiscountInfoDto>> GetCustomerDiscountInfo(int customerId)
        {
            try
            {
                // 1. Получаем информацию о скидке из сервиса
                var discountInfo = await _orderService.GetCustomerDiscountInfoAsync(customerId);
                if (discountInfo == null)
                    return NotFound();
                var dto = new DiscountInfoDto
                {
                    CurrentDiscountPercent = discountInfo.CurrentDiscountPercent,
                    TotalSpent = discountInfo.TotalSpent,
                    AmountToNextLevel = discountInfo.AmountToNextLevel,
                    NextLevelThreshold = discountInfo.NextLevelThreshold,
                    DiscountTier = discountInfo.DiscountTier,
                };
                return dto;

                // 2. Если клиент не найден (сервис вернул null)
                if (discountInfo == null)
                    return NotFound();

                return Ok(discountInfo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
