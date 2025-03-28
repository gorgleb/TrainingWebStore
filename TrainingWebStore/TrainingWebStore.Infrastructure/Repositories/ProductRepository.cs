using Microsoft.EntityFrameworkCore;
using TrainingWebStore.Core.Interfaces;
using TrainingWebStore.Core.Models;
using TrainingWebStore.Infrastructure.Data;

namespace TrainingWebStore.Infrastructure.Repositories
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        public ProductRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IReadOnlyList<Product>> GetProductsByCategoryAsync(int categoryId)
        {
            return await _context.Products
                .Where(p => p.CategoryId == categoryId)
                .Include(p => p.Category)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Product>> SearchProductsAsync(string searchTerm)
        {
            // Для PostgreSQL используем функцию ILIKE для поиска без учета регистра
            searchTerm = searchTerm.ToLower();
            return await _context.Products
                .Where(p =>EF.Functions.ILike(p.Name, $"%{searchTerm}%") ||
                           EF.Functions.ILike(p.Description, $"%{searchTerm}%"))
                .Include(p => p.Category)
                .ToListAsync();
        }
    }
}
