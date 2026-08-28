using SQLite;
using PosApp.Models;

namespace PosApp.Services
{
    public class PosDatabase
    {
        SQLiteAsyncConnection? _database = null;

        private async Task Init()
        {
            if (_database != null)
                return;

            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "posapp.db3");
            _database = new SQLiteAsyncConnection(dbPath);

            await _database.CreateTableAsync<Product>();
            await _database.CreateTableAsync<Order>();

            // --- SEED DEFAULT PRODUCTS IF CATALOG IS EMPTY ---
            var productCount = await _database.Table<Product>().CountAsync();
            if (productCount == 0)
            {
                var defaultProducts = new List<Product>
                {
                    new Product { Name = "Espresso Coffee", Category = "Beverages", Price = 3.50m, Stock = 50 },
                    new Product { Name = "Cappuccino", Category = "Beverages", Price = 4.50m, Stock = 40 },
                    new Product { Name = "Croissant", Category = "Bakery", Price = 2.75m, Stock = 30 },
                    new Product { Name = "Blueberry Muffin", Category = "Bakery", Price = 3.00m, Stock = 25 },
                    new Product { Name = "Avocado Toast", Category = "Food", Price = 8.50m, Stock = 15 },
                    new Product { Name = "Club Sandwich", Category = "Food", Price = 9.25m, Stock = 20 }
                };

                await _database.InsertAllAsync(defaultProducts);
            }
        }

        public async Task<List<Product>> GetProductsAsync()
        {
            await Init();
            return await _database.Table<Product>().ToListAsync();
        }

        public async Task<int> SaveProductAsync(Product product)
        {
            await Init();
            if (product.Id != 0)
                return await _database.UpdateAsync(product);
            else
                return await _database.InsertAsync(product);
        }

        public async Task<int> DeleteProductAsync(Product product)
        {
            await Init();
            return await _database.DeleteAsync(product);
        }

        // --- ORDER METHODS FOR SALES HISTORY ---
        public async Task<List<Order>> GetOrdersAsync()
        {
            await Init();
            return await _database.Table<Order>().OrderByDescending(o => o.OrderDate).ToListAsync();
        }

        public async Task<int> SaveOrderAsync(Order order)
        {
            await Init();
            return await _database.InsertAsync(order);
        }
    }
}