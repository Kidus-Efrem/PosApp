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

                // Strict block: if typed value exceeds stock, abort and notify
                if (product.SelectedQuantity > product.Stock)
                {
                    if (Application.Current?.Windows.FirstOrDefault()?.Page is Page currentPage)
                    {
                        await currentPage.DisplayAlert("Invalid Quantity", $"Cannot set quantity higher than available stock ({product.Stock}).", "OK");
                    }
                    product.SelectedQuantity = product.Stock; // Reset input field to max allowable stock
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

                CartItems.Clear();
                CalculateGrandTotal();

                if (Application.Current?.Windows.FirstOrDefault()?.Page is Page currentPage)
                {
                    await currentPage.DisplayAlert("Success", "Sale completed successfully!", "OK");
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