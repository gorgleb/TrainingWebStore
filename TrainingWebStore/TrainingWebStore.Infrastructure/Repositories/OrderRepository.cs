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

        public async Task<Order> GetOrderWithItemsAsync(int orderId)
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
    }
}

