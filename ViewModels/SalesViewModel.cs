using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PosApp.Models;
using PosApp.Services;
using System.Collections.ObjectModel;

namespace PosApp.ViewModels;

public partial class SalesViewModel : ObservableObject
{
    private readonly PosDatabase _database;

    public ObservableCollection<Product> Products { get; } = new();
    public ObservableCollection<SalesItem> CartItems { get; } = new();

    private decimal subTotal;
    public decimal SubTotal
    {
        get => subTotal;
        set
        {
            if (SetProperty(ref subTotal, value))
            {
                OnPropertyChanged(nameof(GrandTotal));
            }
        }
    }

    public decimal GrandTotal => SubTotal;

    public SalesViewModel()
    {
        _database = new PosDatabase();
        _ = LoadCatalogAsync();
    }

    [RelayCommand]
    public async Task LoadCatalogAsync()
    {
        var list = await _database.GetProductsAsync();
        Products.Clear();
        foreach (var p in list)
        {
            Products.Add(p);
        }
    }

    [RelayCommand]
    public void AddToCart(Product? product)
    {
        if (product == null) return;

        var existingItem = CartItems.FirstOrDefault(c => c.ProductId == product.Id);
        if (existingItem != null)
        {
            if (existingItem.Quantity < product.Stock)
            {
                existingItem.Quantity++;
            }
        }
        else if (product.Stock > 0)
        {
            CartItems.Add(new SalesItem
            {
                ProductId = product.Id,
                Name = product.Name,
                Price = product.Price,
                Quantity = 1
            });
        }
        CalculateTotals();
    }

    [RelayCommand]
    public void RemoveFromCart(SalesItem? item)
    {
        if (item == null) return;

        if (item.Quantity > 1)
        {
            item.Quantity--;
        }
        else
        {
            CartItems.Remove(item);
        }
        CalculateTotals();
    }

    private void CalculateTotals()
    {
        SubTotal = CartItems.Sum(i => i.TotalPrice);
    }

    [RelayCommand]
    public async Task CompleteCheckoutAsync()
    {
        if (!CartItems.Any()) return;

        CartItems.Clear();
        CalculateTotals();
        await LoadCatalogAsync();
    }
}