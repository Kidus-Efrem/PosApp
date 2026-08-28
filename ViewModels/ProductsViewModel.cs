using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using PosApp.Models;
using PosApp.Services;

namespace PosApp.ViewModels
{
    public class ProductsViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Product> Products { get; set; } = new();

        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        private decimal _price;
        public decimal Price
        {
            get => _price;
            set { _price = value; OnPropertyChanged(); }
        }

        private int _stock;
        public int Stock
        {
            get => _stock;
            set { _stock = value; OnPropertyChanged(); }
        }

        public ICommand AddProductCommand { get; }
        public ICommand DeleteProductCommand { get; }

        public ProductsViewModel()
        {
            // Load products immediately upon creation
            _ = LoadProductsAsync();

            AddProductCommand = new Command(async () =>
            {
                if (string.IsNullOrWhiteSpace(Name)) return;

                var newProduct = new Product
                {
                    Name = Name,
                    Price = Price,
                    Stock = Stock,
                    SelectedQuantity = 1
                };

                var database = new PosDatabase();
                await database.SaveProductAsync(newProduct);

                // Clear form fields
                Name = string.Empty;
                Price = 0;
                Stock = 0;

                await LoadProductsAsync();
            });

            DeleteProductCommand = new Command<Product>(async product =>
            {
                if (product == null) return;

                var database = new PosDatabase();
                await database.DeleteProductAsync(product);
                await LoadProductsAsync();
            });
        }

        public async Task LoadProductsAsync()
        {
            var database = new PosDatabase();
            var list = await database.GetProductsAsync();

            Products.Clear();
            foreach (var p in list)
            {
                Products.Add(p);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null!) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}