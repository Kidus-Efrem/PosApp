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
        public ObservableCollection<Product> Products { get; set; } = new();
        public ObservableCollection<SalesItem> CartItems { get; set; } = new();

        private decimal _grandTotal;
        public decimal GrandTotal
        {
            get => _grandTotal;
            set { _grandTotal = value; OnPropertyChanged(); }
        }

        public ICommand IncreaseSelectedQtyCommand { get; }
        public ICommand DecreaseSelectedQtyCommand { get; }
        public ICommand SetCartQuantityCommand { get; }
        public ICommand RemoveFromCartCommand { get; }
        public ICommand CompleteCheckoutCommand { get; }

        public SalesViewModel()
        {
            // Load products immediately upon instantiation so they appear right away
            _ = LoadCatalogAsync();

            DecreaseSelectedQtyCommand = new Command<Product>(product =>
            {
                if (product != null && product.SelectedQuantity > 0)
                {
                    product.SelectedQuantity--;
                }
            });

            IncreaseSelectedQtyCommand = new Command<Product>(async product =>
            {
                if (product != null)
                {
                    if (product.SelectedQuantity < product.Stock)
                    {
                        product.SelectedQuantity++;
                    }
                    else
                    {
                        if (Application.Current?.Windows.FirstOrDefault()?.Page is Page currentPage)
                        {
                            await currentPage.DisplayAlert("Stock Limit", $"Cannot exceed available stock ({product.Stock}).", "OK");
                        }
                    }
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

                if (product.SelectedQuantity < 0)
                {
                    product.SelectedQuantity = 0;
                }

                var existingItem = CartItems.FirstOrDefault(x => x.ProductId == product.Id);

                if (product.SelectedQuantity == 0)
                {
                    if (existingItem != null)
                    {
                        CartItems.Remove(existingItem);
                    }
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
                    CalculateGrandTotal();
                }
            });

            CompleteCheckoutCommand = new Command(async () =>
            {
                if (CartItems.Count == 0) return;

                var database = new PosDatabase();

                // 1. Deduct stock and save products to database
                foreach (var cartItem in CartItems)
                {
                    var productInCatalog = Products.FirstOrDefault(p => p.Id == cartItem.ProductId);
                    if (productInCatalog != null)
                    {
                        productInCatalog.Stock -= cartItem.Quantity;
                        if (productInCatalog.Stock < 0) productInCatalog.Stock = 0;

                        productInCatalog.SelectedQuantity = 1;

                        await database.SaveProductAsync(productInCatalog);
                    }
                }

                // 2. Save the completed Order record for Sales History
                var newOrder = new Order
                {
                    TotalAmount = GrandTotal,
                    OrderDate = DateTime.Now
                };
                await database.SaveOrderAsync(newOrder);

                // 3. Create a snapshot copy of the cart and total to pass to the Receipt Modal
                var receiptItems = new ObservableCollection<SalesItem>(CartItems);
                var receiptTotal = GrandTotal;

                // 4. Clear cart and reset totals for the next customer
                CartItems.Clear();
                CalculateGrandTotal();

                // 5. Pop up the receipt modal
                if (Application.Current?.Windows.FirstOrDefault()?.Page is Page currentPage)
                {
                    await currentPage.Navigation.PushModalAsync(new Views.ReceiptPopupPage(receiptItems, receiptTotal));
                }
            });
        }

        public void CalculateGrandTotal()
        {
            GrandTotal = CartItems.Sum(x => x.TotalPrice);
        }

        public async Task LoadCatalogAsync()
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