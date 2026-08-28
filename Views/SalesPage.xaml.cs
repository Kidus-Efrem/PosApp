using PosApp.ViewModels;

namespace PosApp.Views;

public partial class SalesPage : ContentPage
{
    private SalesViewModel _viewModel;

    public SalesPage()
    {
        InitializeComponent();
        _viewModel = new SalesViewModel();
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadCatalogAsync();
    }
}