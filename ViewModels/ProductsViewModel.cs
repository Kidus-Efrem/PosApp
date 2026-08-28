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
        private List<Product> _allProducts = new();
        public ObservableCollection<Product> Products { get; set; } = new();

        private string _name = string.Empty;
        public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }

        private string _category = string.Empty;
        public string Category { get => _category; set { _category = value; OnPropertyChanged(); } }

        private decimal? _price;
        public decimal? Price { get => _price; set { _price = value; OnPropertyChanged(); } }

        private int? _stock;
        public int? Stock { get => _stock; set { _stock = value; OnPropertyChanged(); } }

        private string _searchQuery = string.Empty;
        public string SearchQuery { get => _searchQuery; set { _searchQuery = value; OnPropertyChanged(); ApplyFilters(); } }

        private string _selectedFilterCategory = "All Categories";
        public string SelectedFilterCategory { get => _selectedFilterCategory; set { _selectedFilterCategory = value; OnPropertyChanged(); ApplyFilters(); } }

        // SEPARATE SORTING PROPERTIES
        private string _nameSortOption = "Default";
        public string NameSortOption { get => _nameSortOption; set { _nameSortOption = value; OnPropertyChanged(); ApplyFilters(); } }

        private string _priceSortOption = "Default";
        public string PriceSortOption { get => _priceSortOption; set { _priceSortOption = value; OnPropertyChanged(); ApplyFilters(); } }

        public ObservableCollection<string> FilterCategories { get; } = new();

        public ICommand AddProductCommand { get; }
        public ICommand DeleteProductCommand { get; }
        public ICommand FilterCategoryCommand { get; }
        public ICommand ToggleNameSortCommand { get; }
        public ICommand TogglePriceSortCommand { get; }

        public ProductsViewModel()
        {
            _ = LoadProductsAsync();

            FilterCategoryCommand = new Command<string>(category =>
            {
                if (!string.IsNullOrEmpty(category)) SelectedFilterCategory = category;
            });

            ToggleNameSortCommand = new Command(() =>
            {
                NameSortOption = NameSortOption switch
                {
                    "Default" => "A to Z",
                    "A to Z" => "Z to A",
                    _ => "Default"
                };
            });

            TogglePriceSortCommand = new Command(() =>
            {
                PriceSortOption = PriceSortOption switch
                {
                    "Default" => "Low to High",
                    "Low to High" => "High to Low",
                    _ => "Default"
                };
            });

            AddProductCommand = new Command(async () =>
            {
                if (string.IsNullOrWhiteSpace(Name) || !Price.HasValue || !Stock.HasValue) return;

                var newProduct = new Product
                {
                    Name = Name,
                    Category = string.IsNullOrWhiteSpace(Category) ? "General" : Category,
                    Price = Price.Value,
                    Stock = Stock.Value,
                    SelectedQuantity = 1
                };

                var database = new PosDatabase();
                await database.SaveProductAsync(newProduct);

                Name = string.Empty; Category = string.Empty; Price = null; Stock = null;
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
            _allProducts = await database.GetProductsAsync();

            var uniqueCategories = _allProducts
                .Select(p => p.Category)
                .Distinct()
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .ToList();

            FilterCategories.Clear();
            FilterCategories.Add("All Categories");
            foreach (var cat in uniqueCategories) FilterCategories.Add(cat);

            if (!FilterCategories.Contains(SelectedFilterCategory)) SelectedFilterCategory = "All Categories";

            ApplyFilters();
        }

        private void ApplyFilters()
        {
            var filtered = _allProducts.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchQuery))
                filtered = filtered.Where(p => p.Name.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase));

            if (SelectedFilterCategory != "All Categories" && !string.IsNullOrEmpty(SelectedFilterCategory))
                filtered = filtered.Where(p => p.Category == SelectedFilterCategory);

            // Apply Price Sort First
            if (PriceSortOption == "Low to High")
                filtered = filtered.OrderBy(p => p.Price);
            else if (PriceSortOption == "High to Low")
                filtered = filtered.OrderByDescending(p => p.Price);

            // Apply Name Sort Second (Secondary Sort if Price is active)
            if (NameSortOption == "A to Z")
                filtered = PriceSortOption == "Default" ? filtered.OrderBy(p => p.Name) : ((IOrderedEnumerable<Product>)filtered).ThenBy(p => p.Name);
            else if (NameSortOption == "Z to A")
                filtered = PriceSortOption == "Default" ? filtered.OrderByDescending(p => p.Name) : ((IOrderedEnumerable<Product>)filtered).ThenByDescending(p => p.Name);

            // Default fallback if neither are active
            if (PriceSortOption == "Default" && NameSortOption == "Default")
                filtered = filtered.OrderBy(p => p.Id);

            Products.Clear();
            foreach (var p in filtered) Products.Add(p);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null!) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}