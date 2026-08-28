namespace PosApp.Views;

public partial class ProductsPage : ContentPage
{
    public ProductsPage()
    {
        InitializeComponent();

        // Explicitly set the BindingContext so data and commands bind correctly
        BindingContext = new ViewModels.ProductsViewModel();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ViewModels.ProductsViewModel vm)
        {
            await vm.LoadProductsAsync();
        }
    }

    private async void OnNavigateToProducts(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("///ProductsPage");
    }

    private async void OnNavigateToSales(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("///SalesPage");
    }
}