using Microsoft.EntityFrameworkCore;
using TrainingWebStore.Core.Enums;
using TrainingWebStore.Core.Interfaces;
using TrainingWebStore.Core.Models;
using TrainingWebStore.Infrastructure.Data;

namespace TrainingWebStore.Infrastructure.Repositories
{
    public class OrderRepository : Repository<Order>, IOrderRepository
    {
        public OrderRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IReadOnlyList<Order>> GetOrdersByCustomerAsync(int customerId)
        {
            return await _context.Orders
                .Where(o => o.CustomerId == customerId)
                .Include(o => o.Customer)
                .ToListAsync();
        }

        public async Task<Order?> GetOrderWithItemsAsync(int orderId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task<IReadOnlyList<Order>> GetCompletedOrdersByCustomerAsync(int customerId)
        {
            return await _context.Orders
                .Where(o => o.CustomerId == customerId && o.Status == OrderStatus.Delivered)
                .Include(o => o.OrderItems)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Order>> GetOrdersByPeriodAsync(
    DateTime startDate,
    DateTime endDate,
    bool includeItems = true,
    bool includeProducts = true)
        {
            // Создаем базовый запрос
            IQueryable<Order> query = _context.Orders
                .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
                .Include(o => o.Customer); // Всегда включаем информацию о клиенте

            // Добавляем включение элементов заказа если требуется
            if (includeItems)
            {
                query = query.Include(o => o.OrderItems);
            }

            // Если нужно включить информацию о продуктах и категориях
            if (includeItems && includeProducts)
            {
                query = query
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                            .ThenInclude(p => p.Category);
            }

            return await query.ToListAsync();
        }
        public async Task<IEnumerable<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            // Включаем связанные данные (товары, продукты, категории)
            return await _context.Orders
                .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
                .Include(o => o.OrderItems)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p.Category)
                .Include(o => o.Customer)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
