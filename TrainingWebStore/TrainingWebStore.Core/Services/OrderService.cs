
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

        // 1. Основные методы работы с заказами
        public async Task<Order> GetOrderByIdAsync(int id)
        {
            return await _orderRepository.GetOrderWithItemsAsync(id);
        }

        public async Task<IReadOnlyList<Order>> GetOrdersByCustomerAsync(int customerId)
        {
            return await _orderRepository.GetOrdersByCustomerAsync(customerId);
        }

        public async Task<Order> CreateOrderAsync(Order order)
        {
            decimal subtotal = 0;
            foreach (var item in order.OrderItems)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId);
                if (product == null)
                    throw new Exception($"Product with ID {item.ProductId} not found");

                if (product.StockQuantity < item.Quantity)
                    throw new Exception($"Not enough stock for product {product.Name}");

                item.UnitPrice = product.Price;
                subtotal += item.UnitPrice * item.Quantity;
                product.StockQuantity -= item.Quantity;
                await _productRepository.UpdateAsync(product);
            }

            order.SubtotalAmount = subtotal;
            order.DiscountPercentage = await CalculateCustomerDiscountAsync(order.CustomerId);
            order.DiscountAmount = subtotal * (order.DiscountPercentage / 100);
            order.OrderDate = DateTime.UtcNow;
            order.Status = OrderStatus.Pending;

            return await _orderRepository.AddAsync(order);
        }

        // 2. Методы для системы скидок
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
                    DiscountTier = GetDiscountTier(discountPercent)
                };
            }
            catch
            {
                return null;
            }
        }

        public async Task<decimal> GetCustomerTotalSpentAsync(int customerId)
        {
            var completedOrders = await _orderRepository.GetCompletedOrdersByCustomerAsync(customerId);
            return completedOrders.Sum(o => o.TotalAmount);
        }

        // 3. Методы для работы со статусами заказов
        public async Task UpdateOrderStatusAsync(int orderId, OrderStatus newStatus)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
                throw new Exception($"Order with ID {orderId} not found");

            order.Status = newStatus;
            await _orderRepository.UpdateAsync(order);
        }

        public async Task<SalesSummary> GetSalesSummaryAsync(
            DateTime startDate,
            DateTime endDate,
            bool compareWithPrevious = false)
        {
            var orders = await _orderRepository.GetOrdersByPeriodAsync(
                startDate,
                endDate,
                includeItems: true,
                includeProducts: true);

            var totalSales = orders.Sum(o => o.TotalAmount);
            var totalOrders = orders.Count;
            var averageOrderValue = totalOrders > 0 ? totalSales / totalOrders : 0;
            var completedOrders = orders.Count(o => o.Status == OrderStatus.Delivered);
            var cancelledOrders = orders.Count(o => o.Status == OrderStatus.Cancelled);

            var topCategories = orders
                .SelectMany(o => o.OrderItems)
                .Where(oi => oi.Product?.Category != null)
                .GroupBy(oi => oi.Product.Category)
                .Select(g => new CategorySales
                {
                    CategoryId = g.Key.Id,
                    CategoryName = g.Key.Name,
                    SalesAmount = g.Sum(oi => oi.Quantity * oi.UnitPrice),
                    OrdersCount = g.Select(oi => oi.OrderId).Distinct().Count()
                })
                .OrderByDescending(c => c.SalesAmount)
                .Take(3)
                .ToList();

            var result = new SalesSummary
            {
                Period = new PeriodInfo(startDate, endDate),
                TotalSales = totalSales,
                TotalOrders = totalOrders,
                AverageOrderValue = averageOrderValue,
                CompletedOrders = completedOrders,
                CancelledOrders = cancelledOrders,
                ConversionRate = CalculateConversionRate(orders),
                TopCategories = topCategories
            };

            if (compareWithPrevious)
            {
                result.Comparison = await GetComparisonDataAsync(
                    startDate,
                    endDate,
                    totalSales,
                    totalOrders);
            }

            return result;
        }


        // 5. Вспомогательные методы
        private async Task<ComparisonInfo> GetComparisonDataAsync(
            DateTime startDate,
            DateTime endDate,
            decimal currentTotalSales,
            int currentTotalOrders)
        {
            var duration = endDate - startDate;
            var previousEndDate = startDate.AddDays(-1);
            var previousStartDate = previousEndDate.AddDays(-duration.Days);

            var previousOrders = await _orderRepository.GetOrdersByPeriodAsync(
                previousStartDate,
                previousEndDate,
                includeItems: false);

            var previousTotalSales = previousOrders.Sum(o => o.TotalAmount);
            var previousTotalOrders = previousOrders.Count;

            return new ComparisonInfo
            {
                PreviousPeriod = new PreviousPeriodInfo {
                    StartDate =  previousStartDate,
                    EndDate = previousEndDate,
                    TotalSales = previousTotalSales,
                    TotalOrders = previousTotalOrders},
                SalesGrowth = previousTotalSales > 0 ?
                    (currentTotalSales - previousTotalSales) / previousTotalSales * 100 : 0,
                OrdersGrowth = previousTotalOrders > 0 ?
                    (currentTotalOrders - previousTotalOrders) / previousTotalOrders * 100 : 0
            };
        }

        private async Task<decimal> CalculateCustomerDiscountAsync(int customerId)
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
            _ => 0
        };

        private string GetDiscountTier(decimal discountPercent) => discountPercent switch
        {
            15 => "Gold",
            10 => "Silver",
            5 => "Bronze",
            _ => "No discount"
        };

        private decimal CalculateConversionRate(IReadOnlyList<Order> orders)
        {
            // TODO: Заменить на реальный расчет конверсии
            return orders.Count > 0 ? 3.2m : 0;
        }
        public async Task<IEnumerable<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var orders = await GetOrdersByDateRangeAsync(startDate, endDate);
            return orders;
        }
        public async Task UpdateOrderStatusAsyncs(int orderId, OrderStatus newStatus)
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
