using CommunityToolkit.Mvvm.ComponentModel;

namespace PosApp.Models
{
    public partial class SalesItem : ObservableObject
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = "General";
        public decimal Price { get; set; }

        private int quantity;
        public int Quantity
        {
            get => quantity;
            set
            {
                if (SetProperty(ref quantity, value))
                {
                    OnPropertyChanged(nameof(TotalPrice));
                }
            }
        }

        public decimal TotalPrice => Price * Quantity;
    }
}