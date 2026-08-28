using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PosApp.Models;
using PosApp.Services;
using System.Collections.ObjectModel;

namespace PosApp.ViewModels;

public partial class ProductsViewModel : ObservableObject
{
    private readonly PosDatabase _database;

    public ObservableCollection<Product> Products { get; } = new();

    private string name = string.Empty;
    public string Name
    {
        get => name;
        set => SetProperty(ref name, value);
    }

    private decimal price;
    public decimal Price
    {
        get => price;
        set => SetProperty(ref price, value);
    }

    private int stock;
    public int Stock
    {
        get => stock;
        set => SetProperty(ref stock, value);
    }

    public ProductsViewModel()
    {
        _database = new PosDatabase();
        _ = LoadProductsAsync();
    }

    [RelayCommand]
    public async Task LoadProductsAsync()
    {
        var list = await _database.GetProductsAsync();
        Products.Clear();
        foreach (var p in list)
        {
            Products.Add(p);
        }
    }

    [RelayCommand]
    public async Task AddProductAsync()
    {
        if (string.IsNullOrWhiteSpace(Name)) return;

        var newProduct = new Product
        {
            Name = Name,
            Price = Price,
            Stock = Stock
        };

        await _database.SaveProductAsync(newProduct);

        Name = string.Empty;
        Price = 0;
        Stock = 0;

        await LoadProductsAsync();
    }

    [RelayCommand]
    public async Task DeleteProductAsync(Product? product)
    {
        if (product == null) return;

        await _database.DeleteProductAsync(product);
        await LoadProductsAsync();
    }
}