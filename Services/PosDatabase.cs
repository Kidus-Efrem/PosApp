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
    }
}