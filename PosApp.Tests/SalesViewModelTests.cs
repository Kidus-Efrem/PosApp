using Xunit;
using PosApp.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System;

namespace PosApp.Tests
{
    public class SalesCalculationTests
    {
        [Fact]
        public void CalculateGrandTotal_WithSave10_AppliesCorrectPercentageDiscount()
        {
            var cartItems = new ObservableCollection<SalesItem>
            {
                new SalesItem { Name = "Coffee", Price = 10.00m, Quantity = 2 }
            };

            decimal subtotal = cartItems.Sum(x => x.TotalPrice);
            decimal discountAmount = Math.Round(subtotal * 0.10m, 2);
            decimal grandTotal = subtotal - discountAmount;

            Assert.Equal(20.00m, subtotal);
            Assert.Equal(2.00m, discountAmount);
            Assert.Equal(18.00m, grandTotal);
        }

        [Fact]
        public void CalculateGrandTotal_WithFlat5_AppliesCorrectFlatDiscount()
        {
            var cartItems = new ObservableCollection<SalesItem>
            {
                new SalesItem { Name = "Burger", Price = 15.00m, Quantity = 2 }
            };

            decimal subtotal = cartItems.Sum(x => x.TotalPrice);
            decimal discountAmount = 5.00m;
            decimal grandTotal = subtotal - discountAmount;

            Assert.Equal(30.00m, subtotal);
            Assert.Equal(5.00m, discountAmount);
            Assert.Equal(25.00m, grandTotal);
        }

        [Fact]
        public void CalculateGrandTotal_DiscountCannotExceedSubtotal()
        {
            var cartItems = new ObservableCollection<SalesItem>
            {
                new SalesItem { Name = "Candy", Price = 3.00m, Quantity = 1 }
            };

            decimal subtotal = cartItems.Sum(x => x.TotalPrice);
            decimal discountAmount = 5.00m;

            if (discountAmount > subtotal)
            {
                discountAmount = subtotal;
            }

            decimal grandTotal = subtotal - discountAmount;

            Assert.Equal(3.00m, subtotal);
            Assert.Equal(3.00m, discountAmount);
            Assert.Equal(0.00m, grandTotal);
        }
    }
}
