namespace PosApp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute("SalesPage", typeof(Views.SalesPage));
        Routing.RegisterRoute("ProductsPage", typeof(Views.ProductsPage));
    }
}