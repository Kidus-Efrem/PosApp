using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using PosApp.Models;
using PosApp.Services;

namespace PosApp.ViewModels
{
    public class SalesViewModel : INotifyPropertyChanged
    {
        private List<Product> _allProducts = new();
        public ObservableCollection<Product> Products { get; set; } = new();
        public ObservableCollection<SalesItem> CartItems { get; set; } = new();

        private decimal _grandTotal;
        public decimal GrandTotal
        {
            get => _grandTotal;
            set { _grandTotal = value; OnPropertyChanged(); }
        }

        private decimal _taxAmount;
        public decimal TaxAmount
        {
            get => _taxAmount;
            set { _taxAmount = value; OnPropertyChanged(); }
        }

        // --- FILTER & SORT PROPERTIES ---
        private string _searchQuery = string.Empty;
        public string SearchQuery { get => _searchQuery; set { _searchQuery = value; OnPropertyChanged(); ApplyFilters(); } }

        private string _selectedFilterCategory = "All Categories";
        public string SelectedFilterCategory { get => _selectedFilterCategory; set { _selectedFilterCategory = value; OnPropertyChanged(); ApplyFilters(); } }

        private string _nameSortOption = "Default";
        public string NameSortOption { get => _nameSortOption; set { _nameSortOption = value; OnPropertyChanged(); ApplyFilters(); } }

        private string _priceSortOption = "Default";
        public string PriceSortOption { get => _priceSortOption; set { _priceSortOption = value; OnPropertyChanged(); ApplyFilters(); } }

        public ObservableCollection<string> FilterCategories { get; } = new();

        // --- PAYMENT METHOD PROPERTIES ---
        private string _selectedPaymentMethod = "Cash";
        public string SelectedPaymentMethod
        {
            get => _selectedPaymentMethod;
            set
            {
                _selectedPaymentMethod = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsCashSelected));
                OnPropertyChanged(nameof(IsCardSelected));
            }
        }

        public bool IsCashSelected => SelectedPaymentMethod == "Cash";
        public bool IsCardSelected => SelectedPaymentMethod == "Card";

        // --- PROMO CODE PROPERTIES ---
        private string _promoCodeInput = string.Empty;
        public string PromoCodeInput
        {
            get => _promoCodeInput;
            set { _promoCodeInput = value; OnPropertyChanged(); }
        }

        private decimal _discountAmount;
        public decimal DiscountAmount
        {
            get => _discountAmount;
            set { _discountAmount = value; OnPropertyChanged(); }
        }

        private string _appliedPromoName = string.Empty;
        public string AppliedPromoName
        {
            get => _appliedPromoName;
            set
            {
                _appliedPromoName = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasPromoApplied));
                OnPropertyChanged(nameof(ShowPromoInput));
            }
        }

        public bool HasPromoApplied => !string.IsNullOrEmpty(AppliedPromoName);
        public bool ShowPromoInput => string.IsNullOrEmpty(AppliedPromoName);

        private decimal _subtotal;
        public decimal Subtotal
        {
            get => _subtotal;
            set { _subtotal = value; OnPropertyChanged(); }
        }

        // --- COMMANDS ---
        public ICommand IncreaseSelectedQtyCommand { get; }
        public ICommand DecreaseSelectedQtyCommand { get; }
        public ICommand SetCartQuantityCommand { get; }
        public ICommand RemoveFromCartCommand { get; }
        public ICommand CompleteCheckoutCommand { get; }

        public ICommand FilterCategoryCommand { get; }
        public ICommand ToggleNameSortCommand { get; }
        public ICommand TogglePriceSortCommand { get; }
        public ICommand SelectPaymentMethodCommand { get; }
        public ICommand ApplyPromoCommand { get; }
        public ICommand RemovePromoCommand { get; }

        public SalesViewModel()
        {
            _ = LoadCatalogAsync();

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

            SelectPaymentMethodCommand = new Command<string>(method =>
            {
                if (!string.IsNullOrEmpty(method)) SelectedPaymentMethod = method;
            });

            ApplyPromoCommand = new Command(async () =>
            {
                if (string.IsNullOrWhiteSpace(PromoCodeInput)) return;

                var code = PromoCodeInput.Trim().ToUpper();

                // Compute subtotal first to ensure math is current
                Subtotal = CartItems.Sum(x => x.TotalPrice);

                if (code == "SAVE10")
                {
                    DiscountAmount = Math.Round(Subtotal * 0.10m, 2);
                    AppliedPromoName = "SAVE10 (10% Off)";
                }
                else if (code == "FLAT5")
                {
                    DiscountAmount = 5.00m;
                    AppliedPromoName = "FLAT5 ($5.00 Off)";
                }
                else
                {
                    if (Application.Current?.Windows.FirstOrDefault()?.Page is Page currentPage)
                    {
                        await currentPage.DisplayAlert("Invalid Code", "The promo code entered is invalid.", "OK");
                    }
                    return;
                }

                if (DiscountAmount > Subtotal)
                {
                    DiscountAmount = Subtotal;
                }

                PromoCodeInput = string.Empty;
                CalculateGrandTotal();
            });

            RemovePromoCommand = new Command(() =>
            {
                DiscountAmount = 0;
                AppliedPromoName = string.Empty;
                CalculateGrandTotal();
            });

            DecreaseSelectedQtyCommand = new Command<Product>(product =>
            {
                if (product != null && product.SelectedQuantity > 0)
                    product.SelectedQuantity--;
            });

            IncreaseSelectedQtyCommand = new Command<Product>(async product =>
            {
                if (product != null)
                {
                    if (product.SelectedQuantity < product.Stock)
                        product.SelectedQuantity++;
                    else if (Application.Current?.Windows.FirstOrDefault()?.Page is Page currentPage)
                        await currentPage.DisplayAlert("Stock Limit", $"Cannot exceed available stock ({product.Stock}).", "OK");
                }
            });

            SetCartQuantityCommand = new Command<Product>(async product =>
            {
                if (product == null) return;

                if (product.SelectedQuantity > product.Stock)
                {
                    if (Application.Current?.Windows.FirstOrDefault()?.Page is Page currentPage)
                    {
                        await currentPage.DisplayAlert("Invalid Quantity", $"Cannot set quantity higher than available stock ({product.Stock}).", "OK");
                    }
                    product.SelectedQuantity = product.Stock;
                    return;
                }

                if (product.SelectedQuantity < 0) product.SelectedQuantity = 0;

                var existingItem = CartItems.FirstOrDefault(x => x.ProductId == product.Id);

                if (product.SelectedQuantity == 0)
                {
                    if (existingItem != null) CartItems.Remove(existingItem);
                }
                else
                {
                    if (existingItem != null)
                    {
                        existingItem.Quantity = product.SelectedQuantity;
                    }
                    else
                    {
                        CartItems.Add(new SalesItem
                        {
                            ProductId = product.Id,
                            Name = product.Name,
                            Price = product.Price,
                            Category = product.Category,
                            Quantity = product.SelectedQuantity
                        });
                    }
                }

                CalculateGrandTotal();
            });

            RemoveFromCartCommand = new Command<SalesItem>(item =>
            {
                if (item != null)
                {
                    CartItems.Remove(item);
                    CalculateGrandTotal(); // Re-calculates totals and discounts immediately when item is removed
                }
            });

            CompleteCheckoutCommand = new Command(async () =>
            {
                if (CartItems.Count == 0) return;

                var database = new PosDatabase();

                foreach (var cartItem in CartItems)
                {
                    var productInCatalog = _allProducts.FirstOrDefault(p => p.Id == cartItem.ProductId);
                    if (productInCatalog != null)
                    {
                        productInCatalog.Stock -= cartItem.Quantity;
                        if (productInCatalog.Stock < 0) productInCatalog.Stock = 0;
                        productInCatalog.SelectedQuantity = 1;
                        await database.SaveProductAsync(productInCatalog);
                    }
                }

                var newOrder = new Order
                {
                    TotalAmount = GrandTotal,
                    OrderDate = DateTime.Now
                };
                await database.SaveOrderAsync(newOrder);

                var receiptItems = new ObservableCollection<SalesItem>(CartItems);
                var receiptSubtotal = Subtotal;
                var receiptTax = TaxAmount;
                var receiptTotal = GrandTotal;

                CartItems.Clear();
                DiscountAmount = 0;
                AppliedPromoName = string.Empty;
                CalculateGrandTotal();
                SelectedPaymentMethod = "Cash";
                await LoadCatalogAsync();

                if (Application.Current?.Windows.FirstOrDefault()?.Page is Page currentPage)
                {
                    await currentPage.Navigation.PushModalAsync(new Views.ReceiptPopupPage(receiptItems, receiptSubtotal, receiptTax, receiptTotal));
                }
            });
        }

        public void CalculateGrandTotal()
        {
            Subtotal = CartItems.Sum(x => x.TotalPrice);

            // If a percentage discount like SAVE10 is active, update the discount amount dynamically if items change
            if (AppliedPromoName.Contains("SAVE10"))
            {
                DiscountAmount = Math.Round(Subtotal * 0.10m, 2);
            }

            if (DiscountAmount > Subtotal)
            {
                DiscountAmount = Subtotal;
            }

            // Calculate taxable amount after discount, then compute 8.5% sales tax
            decimal taxableAmount = Subtotal - DiscountAmount;
            TaxAmount = Math.Round(taxableAmount * 0.085m, 2);

            GrandTotal = taxableAmount + TaxAmount;
            if (GrandTotal < 0) GrandTotal = 0;
        }

        public async Task LoadCatalogAsync()
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

            if (PriceSortOption == "Low to High")
                filtered = filtered.OrderBy(p => p.Price);
            else if (PriceSortOption == "High to Low")
                filtered = filtered.OrderByDescending(p => p.Price);

            if (NameSortOption == "A to Z")
                filtered = PriceSortOption == "Default" ? filtered.OrderBy(p => p.Name) : ((IOrderedEnumerable<Product>)filtered).ThenBy(p => p.Name);
            else if (NameSortOption == "Z to A")
                filtered = PriceSortOption == "Default" ? filtered.OrderByDescending(p => p.Name) : ((IOrderedEnumerable<Product>)filtered).ThenByDescending(p => p.Name);

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