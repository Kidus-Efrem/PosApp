using System.ComponentModel;
using System.Runtime.CompilerServices;
using SQLite;

namespace PosApp.Models
{
    public class Product : INotifyPropertyChanged
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        private string _category = "General";
        public string Category
        {
            get => _category;
            set { _category = value; OnPropertyChanged(); }
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

        [Ignore] // Prevents SQLite from saving this UI-only property to the database
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
                    _selectedQuantity = Stock;
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