using TrainingWebStore.Core.Models;

namespace TrainingWebStore.Core.Interfaces
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<IReadOnlyList<Product>> GetProductsByCategoryAsync(int categoryId);
        Task<IReadOnlyList<Product>> SearchProductsAsync(string searchTerm);
    }
}
