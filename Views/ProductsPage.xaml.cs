namespace PosApp.Views;

public partial class ProductsPage : ContentPage
{
    public ProductsPage()
    {
        InitializeComponent();
        BindingContext = new ViewModels.ProductsViewModel();
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        if (width <= 0) return;

        if (width < 900)
        {
            // MOBILE / NARROW VIEW: Stack Form above Catalog
            ResponsiveLayout.ColumnDefinitions.Clear();
            ResponsiveLayout.RowDefinitions.Clear();

            ResponsiveLayout.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            ResponsiveLayout.RowDefinitions.Add(new RowDefinition(GridLength.Auto)); // Form height
            ResponsiveLayout.RowDefinitions.Add(new RowDefinition(GridLength.Star)); // Catalog takes the rest

            Grid.SetColumn(FormContainer, 0);
            Grid.SetRow(FormContainer, 0);

            Grid.SetColumn(CatalogContainer, 0);
            Grid.SetRow(CatalogContainer, 1);
        }
        else
        {
            // DESKTOP / WIDE VIEW: Side by Side
            ResponsiveLayout.ColumnDefinitions.Clear();
            ResponsiveLayout.RowDefinitions.Clear();

            ResponsiveLayout.ColumnDefinitions.Add(new ColumnDefinition(380));
            ResponsiveLayout.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            ResponsiveLayout.RowDefinitions.Add(new RowDefinition(GridLength.Star)); // Binds exactly to window height

            Grid.SetColumn(FormContainer, 0);
            Grid.SetRow(FormContainer, 0);

            Grid.SetColumn(CatalogContainer, 1);
            Grid.SetRow(CatalogContainer, 0);
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