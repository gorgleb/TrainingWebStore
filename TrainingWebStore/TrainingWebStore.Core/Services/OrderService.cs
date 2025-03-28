using TrainingWebStore.Core.Enums;
using TrainingWebStore.Core.Interfaces;
using TrainingWebStore.Core.Models;


namespace TrainingWebStore.Core.Services
{
    public class OrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;

        public OrderService(IOrderRepository orderRepository, IProductRepository productRepository)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
        }

        // 1. Существующие методы (без изменений)
        public async Task<Order> GetOrderByIdAsync(int id)
        {
            return await _orderRepository.GetOrderWithItemsAsync(id);
        }

        public async Task<IReadOnlyList<Order>> GetOrdersByCustomerAsync(int customerId)
        {
            return await _orderRepository.GetOrdersByCustomerAsync(customerId);
        }

        // 2. Обновленный метод создания заказа (добавлен расчет скидки)
        public async Task<Order> CreateOrderAsync(Order order)
        {
            // Проверка товаров и расчет суммы без скидки
            decimal subtotal = 0;
            foreach (var item in order.OrderItems)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId);
                if (product == null)
                {
                    throw new Exception($"Product with ID {item.ProductId} not found");
                }
                if (product.StockQuantity < item.Quantity)
                {
                    throw new Exception($"Not enough stock for product {product.Name}. Available: {product.StockQuantity}, Requested: {item.Quantity}");
                }

                item.UnitPrice = product.Price;
                subtotal += item.UnitPrice * item.Quantity;

                product.StockQuantity -= item.Quantity;
                await _productRepository.UpdateAsync(product);
            }

            // Расчет и применение скидки
            order.SubtotalAmount = subtotal;
            order.DiscountPercentage = await CalculateCustomerDiscountAsync(order.CustomerId);
            order.DiscountAmount = subtotal * (order.DiscountPercentage / 100);
            // TotalAmount вычисляется автоматически в модели Order

            order.OrderDate = DateTime.UtcNow;
            order.Status = OrderStatus.Pending;

            return await _orderRepository.AddAsync(order);
        }

        // 3. Методы для системы скидок
        public async Task<DiscountInfo?> GetCustomerDiscountInfoAsync(int customerId)
        {
            try
            {
                var totalSpent = await GetCustomerTotalSpentAsync(customerId);
                var discountPercent = await CalculateCustomerDiscountAsync(customerId);
                var nextThreshold = GetNextThreshold(totalSpent);

                return new DiscountInfo
                {
                    CurrentDiscountPercent = discountPercent,
                    TotalSpent = totalSpent,
                    AmountToNextLevel = Math.Max(nextThreshold - totalSpent, 0),
                    NextLevelThreshold = nextThreshold,
                    DiscountTier = GetDiscountTier(discountPercent) ?? "No discount"
                };
            }
            catch
            {
                return null; // В случае ошибки вернет null
            }
        }

        public async Task<decimal> GetCustomerTotalSpentAsync(int customerId)
        {
            var completedOrders = await _orderRepository.GetCompletedOrdersByCustomerAsync(customerId);
            return completedOrders.Sum(o => o.TotalAmount);
        }

        public async Task<decimal> CalculateCustomerDiscountAsync(int customerId)
        {
            var totalSpent = await GetCustomerTotalSpentAsync(customerId);
            return totalSpent switch
            {
                >= 10000 => 15,
                >= 5000 => 10,
                >= 1000 => 5,
                _ => 0
            };
        }

        private decimal GetNextThreshold(decimal totalSpent) => totalSpent switch
        {
            < 1000 => 1000,
            < 5000 => 5000,
            < 10000 => 10000,
            _ => 0 // Максимальный уровень
        };

        private string GetDiscountTier(decimal discountPercent) => discountPercent switch
        {
            15 => "Gold",
            10 => "Silver",
            5 => "Bronze",
            _ => "No discount"
        };

        // 4. Существующие методы (без изменений)
        public async Task UpdateOrderStatusAsync(int orderId, OrderStatus newStatus)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
            {
                throw new Exception($"Order with ID {orderId} not found");
            }

            order.Status = newStatus;
            await _orderRepository.UpdateAsync(order);
        }

        public async Task CancelOrderAsync(int orderId)
        {
            var order = await _orderRepository.GetOrderWithItemsAsync(orderId);
            if (order == null)
            {
                throw new Exception($"Order with ID {orderId} not found");
            }

            if (order.Status == OrderStatus.Shipped || order.Status == OrderStatus.Delivered)
            {
                throw new Exception("Cannot cancel an order that has been shipped or delivered");
            }

            // Возвращаем товары на склад
            foreach (var item in order.OrderItems)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId);
                if (product != null)
                {
                    product.StockQuantity += item.Quantity;
                    await _productRepository.UpdateAsync(product);
                }
            }

            order.Status = OrderStatus.Cancelled;
            await _orderRepository.UpdateAsync(order);
        }
    }
}