using Microsoft.Maui.Controls;
using PosApp.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Collections.ObjectModel;
using QColors = QuestPDF.Helpers.Colors;

namespace PosApp.Views;

public partial class ReceiptPopupPage : ContentPage
{
    public DateTime OrderDate { get; set; }
    public ObservableCollection<SalesItem> PurchasedItems { get; set; }
    public decimal ReceiptSubtotal { get; set; }
    public decimal ReceiptTax { get; set; }
    public decimal GrandTotal { get; set; }

    public ReceiptPopupPage(ObservableCollection<SalesItem> items, decimal subtotal, decimal taxAmount, decimal total)
    {
        InitializeComponent();

        QuestPDF.Settings.License = LicenseType.Community;

        OrderDate = DateTime.Now;
        PurchasedItems = new ObservableCollection<SalesItem>(items);
        ReceiptSubtotal = subtotal;
        ReceiptTax = taxAmount;
        GrandTotal = total;

        BindingContext = this;
    }

    private async void OnCloseClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }

    private async void OnExportPdfClicked(object sender, EventArgs e)
    {
        try
        {
            var fileName = $"Receipt_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A5);
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(QColors.White);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header().Text("PosApp Receipt").SemiBold().FontSize(20).FontColor(QColors.Teal.Darken2);

                    page.Content().PaddingVertical(1, Unit.Centimetre).Column(x =>
                    {
                        x.Spacing(10);
                        x.Item().Text($"Date: {OrderDate:g}").FontSize(10).FontColor(QColors.Grey.Darken2);

                        x.Item().LineHorizontal(1).LineColor(QColors.Grey.Lighten2);

                        foreach (var item in PurchasedItems)
                        {
                            x.Item().Row(row =>
                            {
                                row.RelativeItem().Text(item.Name);
                                row.ConstantItem(40).Text($"x{item.Quantity}");
                                row.ConstantItem(60).AlignRight().Text($"${item.TotalPrice:F2}");
                            });
                        }

                        x.Item().LineHorizontal(1).LineColor(QColors.Grey.Lighten2);
                        x.Item().Text($"Subtotal: ${ReceiptSubtotal:F2}").FontSize(11);
                        x.Item().Text($"Sales Tax (8.5%): ${ReceiptTax:F2}").FontSize(11);
                        x.Item().AlignRight().Text($"Total Paid: ${GrandTotal:F2}").SemiBold().FontSize(14).FontColor(QColors.Green.Darken2);
                    });

                    page.Footer().AlignCenter().Text("Thank you for your purchase!");
                });
            })
            .GeneratePdf(filePath);

            await DisplayAlert("PDF Exported", $"Saved to:\n{filePath}", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Export Failed", ex.Message, "OK");
        }
    }
}