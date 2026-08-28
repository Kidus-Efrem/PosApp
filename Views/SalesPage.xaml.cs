namespace PosApp.Views;

public partial class SalesPage : ContentPage
{
    public SalesPage()
    {
        InitializeComponent();
        BindingContext = new ViewModels.SalesViewModel();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ViewModels.SalesViewModel vm)
        {
            await vm.LoadCatalogAsync();
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