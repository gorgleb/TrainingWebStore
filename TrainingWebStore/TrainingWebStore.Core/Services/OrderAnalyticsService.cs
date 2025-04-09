using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrainingWebStore.Core.Enums;
using TrainingWebStore.Core.Interfaces;
using TrainingWebStore.Core.Models;

namespace TrainingWebStore.Core.Services
{
    public class OrderAnalyticsService
    {
        private readonly OrderService _orderService;

        public OrderAnalyticsService(OrderService orderService)
        {
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
        }

        public async Task<SalesSummary> GetSalesSummary(DateTime startDate, DateTime endDate, bool compareWithPrevious)
        {
            if (!IsGetSalesSummaryRequestValid(ref startDate, ref endDate, compareWithPrevious))
            {
                throw new ArgumentException("Invalid date range parameters");
            }

            // Получаем заказы за указанный период
            var orders = await _orderService.GetOrdersByDateRangeAsync(startDate, endDate);

            // Рассчитываем основные метрики
            var salesSummary = CalculateSalesMetrics(orders, startDate, endDate);

            // Если нужно сравнение с предыдущим периодом
            if (compareWithPrevious)
            {
                var previousPeriodMetrics = await GetPreviousPeriodMetrics(startDate, endDate);
                salesSummary.Comparison = CreateComparisonInfo(previousPeriodMetrics, salesSummary);
            }

            // Получаем топ категорий
            salesSummary.TopCategories = GetTopCategories(orders);

            return salesSummary;
        }

        public bool IsGetSalesSummaryRequestValid(ref DateTime startDate, ref DateTime endDate, bool compareWithPrevious)
        {

            if (startDate == null)
                startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            
            if (startDate > endDate)
                return false;

            if (endDate == null)
                endDate = DateTime.Now;

            return true;
        }

        private SalesSummary CalculateSalesMetrics(IEnumerable<Order> orders, DateTime startDate, DateTime endDate)
        {
            var completedOrders = orders.Where(o => o.Status == OrderStatus.Delivered).ToList();
            var cancelledOrders = orders.Where(o => o.Status == OrderStatus.Cancelled).ToList();

            var totalSales = completedOrders.Sum(o => o.TotalAmount);
            var totalOrders = orders.Count();
            var avgOrderValue = totalOrders > 0 ? totalSales / totalOrders : 0;
            var conversionRate = totalOrders > 0 ? (decimal)completedOrders.Count / totalOrders * 100 : 0;

            return new SalesSummary
            {
                Period = new PeriodInfo { StartDate = startDate, EndDate = endDate },
                TotalSales = totalSales,
                TotalOrders = totalOrders,
                AverageOrderValue = avgOrderValue,
                CompletedOrders = completedOrders.Count,
                CancelledOrders = cancelledOrders.Count,
                ConversionRate = conversionRate
            };
        }

        private async Task<SalesSummary> GetPreviousPeriodMetrics(DateTime currentStart, DateTime currentEnd)
        {
            var periodLength = currentEnd - currentStart;
            var previousStart = currentStart - periodLength;
            var previousEnd = currentStart.AddDays(-1);

            var previousOrders = await _orderService.GetOrdersByDateRangeAsync(previousStart, previousEnd);
            return CalculateSalesMetrics(previousOrders, previousStart, previousEnd);
        }

        private ComparisonInfo CreateComparisonInfo(SalesSummary previous, SalesSummary current)
        {
            return new ComparisonInfo
            {
                PreviousPeriod = new PreviousPeriodInfo
                {
                    StartDate = previous.Period.StartDate,
                    EndDate = previous.Period.EndDate,
                    TotalSales = previous.TotalSales,
                    TotalOrders = previous.TotalOrders
                },
                SalesGrowth = previous.TotalSales != 0
                    ? (current.TotalSales - previous.TotalSales) / previous.TotalSales * 100
                    : 100, // Если предыдущих продаж не было, считаем рост 100%
                OrdersGrowth = previous.TotalOrders != 0
                    ? (current.TotalOrders - previous.TotalOrders) / (decimal)previous.TotalOrders * 100
                    : 100
            };
        }

        private List<CategorySales> GetTopCategories(IEnumerable<Order> orders)
        {
            return orders
                .Where(o => o.Status == OrderStatus.Delivered)
                .SelectMany(o => o.OrderItems)
                .GroupBy(i => i.Product.Category)
                .Select(g => new CategorySales
                {
                    CategoryId = g.Key.Id,
                    CategoryName = g.Key.Name,
                    SalesAmount = g.Sum(i => i.UnitPrice * i.Quantity),
                    OrdersCount = g.Select(i => i.OrderId).Distinct().Count()
                })
                .OrderByDescending(c => c.SalesAmount)
                .Take(3)
                .ToList();
        }
    }
}