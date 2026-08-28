using PosApp.ViewModels;

namespace PosApp.Views
{
    public partial class ProductsPage : ContentPage
    {
        private ProductsViewModel _viewModel;

        public ProductsPage()
        {
            InitializeComponent();
            BindingContext = _viewModel = new ProductsViewModel();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadProductsAsync();
        }
    }
}