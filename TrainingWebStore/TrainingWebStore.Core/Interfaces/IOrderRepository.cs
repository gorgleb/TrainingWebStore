using TrainingWebStore.Core.Models;

namespace TrainingWebStore.Core.Interfaces
{
    public interface IOrderRepository : IRepository<Order>
    {

        Task<IReadOnlyList<Order>> GetOrdersByCustomerAsync(int customerId);
        Task<Order> GetOrderWithItemsAsync(int orderId);

        // Новый метод для скидок
        Task<IReadOnlyList<Order>> GetCompletedOrdersByCustomerAsync(int customerId);
    }

}