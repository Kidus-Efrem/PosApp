using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PosApp.Models
{
    public class Product : INotifyPropertyChanged
    {
        public int Id { get; set; }

        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        private decimal _price;
        public decimal Price
        {
            get => _price;
            set { _price = value; OnPropertyChanged(); }
        }

        private int _stock;
        public int Stock
        {
            get => _stock;
            set { _stock = value; OnPropertyChanged(); }
        }

        private int _selectedQuantity = 1;
        public int SelectedQuantity
        {
            get => _selectedQuantity;
            set
            {
                if (value >= 0 && value <= Stock)
                {
                    _selectedQuantity = value;
                    OnPropertyChanged();
                }
                else if (value > Stock)
                {
                    _selectedQuantity = Stock; // Hard cap at maximum stock
                    OnPropertyChanged();
                }
                else if (value < 0)
                {
                    _selectedQuantity = 0;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null!) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}