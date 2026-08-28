namespace PosApp.Views;

public partial class SalesHistoryPage : ContentPage
{
    public SalesHistoryPage()
    {
        InitializeComponent();
        BindingContext = new ViewModels.SalesHistoryViewModel();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ViewModels.SalesHistoryViewModel vm)
        {
            await vm.LoadOrdersAsync();
        }
    }

    private async void OnNavigateToSales(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("///SalesPage");
    }

    private async void OnNavigateToProducts(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("///ProductsPage");
    }

    private async void OnNavigateToHistory(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("///SalesHistoryPage");
    }
}