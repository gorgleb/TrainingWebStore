using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrainingWebStore.Core.Models;

namespace TrainingWebStore.Infrastructure.Data
{
    public class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider, ILogger<SeedData> logger)
        {
            using var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>());

            try
            {
                if (!context.Categories.Any())
                {
                    logger.LogInformation("Seeding categories...");
                    await SeedCategoriesAsync(context);
                }

                if (!context.Products.Any())
                {
                    logger.LogInformation("Seeding products...");
                    await SeedProductsAsync(context);
                }

                if (!context.Customers.Any())
                {
                    logger.LogInformation("Seeding customers...");
                    await SeedCustomersAsync(context);
                }

                logger.LogInformation("Seed completed successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the database.");
            }
        }

        private static async Task SeedCategoriesAsync(ApplicationDbContext context)
        {
            var categories = new List<Category>
            {
                new Category { Name = "Electronics", Description = "Electronic devices and gadgets" },
                new Category { Name = "Clothing", Description = "Apparel and fashion items" },
                new Category { Name = "Books", Description = "Books and literature" },
                new Category { Name = "Home & Kitchen", Description = "Home and kitchen appliances and accessories" },
                new Category { Name = "Sports & Outdoors", Description = "Sports equipment and outdoor gear" }
            };

            await context.Categories.AddRangeAsync(categories);
            await context.SaveChangesAsync();
        }

        private static async Task SeedProductsAsync(ApplicationDbContext context)
        {
            var electronicsCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Electronics");
            var clothingCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Clothing");
            var booksCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Books");
            var homeCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Home & Kitchen");
            var sportsCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Sports & Outdoors");

            var products = new List<Product>
            {
                // Electronics
                new Product
                {
                    Name = "Smartphone X",
                    Description = "Latest smartphone with advanced features",
                    Price = 999.99m,
                    StockQuantity = 50,
                    CategoryId = electronicsCategory.Id
                },
                new Product
                {
                    Name = "Laptop Pro",
                    Description = "High-performance laptop for professionals",
                    Price = 1499.99m,
                    StockQuantity = 30,
                    CategoryId = electronicsCategory.Id
                },
                new Product
                {
                    Name = "Wireless Headphones",
                    Description = "Premium noise-cancelling wireless headphones",
                    Price = 249.99m,
                    StockQuantity = 100,
                    CategoryId = electronicsCategory.Id
                },

                // Clothing
                new Product
                {
                    Name = "Casual T-Shirt",
                    Description = "Comfortable cotton t-shirt for everyday wear",
                    Price = 19.99m,
                    StockQuantity = 200,
                    CategoryId = clothingCategory.Id
                },
                new Product
                {
                    Name = "Denim Jeans",
                    Description = "Classic denim jeans with modern fit",
                    Price = 49.99m,
                    StockQuantity = 150,
                    CategoryId = clothingCategory.Id
                },

                // Books
                new Product
                {
                    Name = "Programming in C#",
                    Description = "Comprehensive guide to C# programming",
                    Price = 39.99m,
                    StockQuantity = 75,
                    CategoryId = booksCategory.Id
                },
                new Product
                {
                    Name = "Web Development Fundamentals",
                    Description = "Learn the basics of web development",
                    Price = 29.99m,
                    StockQuantity = 60,
                    CategoryId = booksCategory.Id
                },

                // Home & Kitchen
                new Product
                {
                    Name = "Coffee Maker",
                    Description = "Automatic coffee maker with timer",
                    Price = 89.99m,
                    StockQuantity = 40,
                    CategoryId = homeCategory.Id
                },
                new Product
                {
                    Name = "Blender",
                    Description = "High-speed blender for smoothies and more",
                    Price = 69.99m,
                    StockQuantity = 35,
                    CategoryId = homeCategory.Id
                },

                // Sports & Outdoors
                new Product
                {
                    Name = "Yoga Mat",
                    Description = "Non-slip yoga mat for home workouts",
                    Price = 24.99m,
                    StockQuantity = 120,
                    CategoryId = sportsCategory.Id
                },
                new Product
                {
                    Name = "Running Shoes",
                    Description = "Lightweight running shoes with cushioned sole",
                    Price = 79.99m,
                    StockQuantity = 80,
                    CategoryId = sportsCategory.Id
                }
            };

            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();
        }

        private static async Task SeedCustomersAsync(ApplicationDbContext context)
        {
            var customers = new List<Customer>
            {
                new Customer
                {
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john.doe@example.com",
                    PhoneNumber = "555-123-4567",
                    Address = "123 Main St, Anytown, USA"
                },
                new Customer
                {
                    FirstName = "Jane",
                    LastName = "Smith",
                    Email = "jane.smith@example.com",
                    PhoneNumber = "555-987-6543",
                    Address = "456 Oak Ave, Somewhere, USA"
                },
                new Customer
                {
                    FirstName = "Bob",
                    LastName = "Johnson",
                    Email = "bob.johnson@example.com",
                    PhoneNumber = "555-456-7890",
                    Address = "789 Pine Rd, Nowhere, USA"
                }
            };

            await context.Customers.AddRangeAsync(customers);
            await context.SaveChangesAsync();
        }
    }
}
