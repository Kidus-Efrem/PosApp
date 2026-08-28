using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PosApp.Models;
using PosApp.Services;
using System.Collections.ObjectModel;

namespace PosApp.ViewModels
{
    public partial class ProductsViewModel : ObservableObject
    {
        private readonly PosDatabase _database;

        public ObservableCollection<Product> Products { get; } = new();

        [ObservableProperty]
        string productName;

        [ObservableProperty]
        decimal? productPrice;

        [ObservableProperty]
        int? productStock;

        public ProductsViewModel()
        {
            _database = new PosDatabase();
        }

        [RelayCommand]
        public async Task LoadProductsAsync()
        {
            var productsList = await _database.GetProductsAsync();
            Products.Clear();
            foreach (var product in productsList)
            {
                Products.Add(product);
            }
        }

        [RelayCommand]
        public async Task AddProductAsync()
        {
            if (string.IsNullOrWhiteSpace(ProductName) || !ProductPrice.HasValue || ProductPrice <= 0)
                return;

            var newProduct = new Product
            {
                Name = ProductName,
                Price = ProductPrice ?? 0m,
                Stock = ProductStock ?? 0
            };

            await _database.SaveProductAsync(newProduct);

            // Clear inputs
            ProductName = string.Empty;
            ProductPrice = null;
            ProductStock = null;

            await LoadProductsAsync();
        }

        [RelayCommand]
        public async Task DeleteProductAsync(Product product)
        {
            if (product != null)
            {
                await _database.DeleteProductAsync(product);
                await LoadProductsAsync();
            }
        }
    }
}