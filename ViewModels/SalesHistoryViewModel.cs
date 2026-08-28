using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using PosApp.Models;
using PosApp.Services;

namespace PosApp.ViewModels
{
    public class SalesHistoryViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Order> Orders { get; set; } = new();

        public SalesHistoryViewModel()
        {
            _ = LoadOrdersAsync();
        }

        public async Task LoadOrdersAsync()
        {
            var database = new PosDatabase();
            var list = await database.GetOrdersAsync();

            Orders.Clear();
            foreach (var order in list)
            {
                Orders.Add(order);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null!) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}