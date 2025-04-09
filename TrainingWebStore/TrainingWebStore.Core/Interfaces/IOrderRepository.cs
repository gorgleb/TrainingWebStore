using TrainingWebStore.Core.Models;

namespace TrainingWebStore.Core.Interfaces
{
    public interface IOrderRepository : IRepository<Order>
    {

        Task<IReadOnlyList<Order>> GetOrdersByCustomerAsync(int customerId);
        Task<Order> GetOrderWithItemsAsync(int orderId);

        // Новый метод для скидок
        Task<IReadOnlyList<Order>> GetCompletedOrdersByCustomerAsync(int customerId);
        Task<IReadOnlyList<Order>> GetOrdersByPeriodAsync(
       DateTime startDate,
       DateTime endDate,
       bool includeItems = true,
       bool includeProducts = true);
       Task<IEnumerable<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate);

    }

}