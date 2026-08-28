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

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        if (width <= 0) return;

        if (width < 900)
        {
            // MOBILE / NARROW VIEW: Stack vertically with auto heights so the page scrolls
            ResponsiveLayout.ColumnDefinitions.Clear();
            ResponsiveLayout.RowDefinitions.Clear();

            ResponsiveLayout.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            ResponsiveLayout.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            ResponsiveLayout.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

            Grid.SetColumn(CatalogContainer, 0);
            Grid.SetRow(CatalogContainer, 0);

            Grid.SetColumn(CartContainer, 0);
            Grid.SetRow(CartContainer, 1);
        }
        else
        {
            // DESKTOP / WIDE VIEW: Side by side
            ResponsiveLayout.ColumnDefinitions.Clear();
            ResponsiveLayout.RowDefinitions.Clear();

            ResponsiveLayout.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            ResponsiveLayout.ColumnDefinitions.Add(new ColumnDefinition(400));
            ResponsiveLayout.RowDefinitions.Add(new RowDefinition(GridLength.Star));

            Grid.SetColumn(CatalogContainer, 0);
            Grid.SetRow(CatalogContainer, 0);

            Grid.SetColumn(CartContainer, 1);
            Grid.SetRow(CartContainer, 0);
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