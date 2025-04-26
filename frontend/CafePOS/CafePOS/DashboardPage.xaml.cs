using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CafePOS.DAO;
using CafePOS.DTO;

namespace CafePOS
{
    public sealed partial class DashboardPage : Page
    {
        public DashboardPage()
        {
            this.InitializeComponent();
            _ = LoadDashboardDataAsync();
        }

        private async Task LoadDashboardDataAsync()
        {
            var products = await DrinkDAO.Instance.GetListDrinkAsync();
            var staffs = await StaffDAO.Instance.GetAllStaffAsync();
            var customers = await GuestDAO.Instance.GetAllGuestsAsync();
            var payments = await PaymentMethodDAO.Instance.GetAllPaymentMethodAsync();
            var result = await DataProvider.Instance.Client.GetAllBills.ExecuteAsync();
            var bills = result.Data?.AllBills?.Edges?.Select(e => e.Node).ToList() ?? new();

            ProductCountText.Text = products.Count.ToString();
            StaffCountText.Text = staffs.Count.ToString();
            CustomerCountText.Text = customers.Count.ToString();
            PaymentCountText.Text = payments.Count.ToString();
            BillCountText.Text = bills.Count.ToString();

            var paidBills = bills.Where(b => b.Status == 1 && !string.IsNullOrEmpty(b.DateCheckOut)).ToList();
            var today = DateTime.Today;

            // Lọc hóa đơn checkout trong ngày hôm nay
            var todayBills = paidBills
                .Where(b =>
                {
                    var checkoutDate = DateTime.Parse(b.DateCheckOut);
                    return checkoutDate.Date == today;
                })
                .ToList();

            double todayRevenue = todayBills.Sum(b => b.TotalAmount);
            int todayTotalBills = todayBills.Count;
            double todayAvgValue = todayTotalBills > 0 ? todayRevenue / todayTotalBills : 0;

            TotalRevenueText.Text = todayRevenue.ToString("C0", new CultureInfo("vi-VN"));
            TotalBillsText.Text = todayTotalBills.ToString();
            AverageBillValueText.Text = todayAvgValue.ToString("C0", new CultureInfo("vi-VN"));

            // Best selling product trong hôm nay
            var productSales = new Dictionary<string, int>();
            foreach (var bill in todayBills)
            {
                if (bill.BillInfosByIdBill?.Nodes != null)
                {
                    foreach (var info in bill.BillInfosByIdBill.Nodes)
                    {
                        var name = info?.ProductByIdProduct?.Name ?? "Unknown";
                        if (!productSales.ContainsKey(name)) productSales[name] = 0;
                        productSales[name] += info?.Count ?? 0;
                    }
                }
            }
            var best = productSales.OrderByDescending(p => p.Value).FirstOrDefault().Key ?? "-";
            BestSellingProductText.Text = best;
        }


        private void ViewProducts_Click(object sender, RoutedEventArgs e)
        {
            (this.Parent as Frame)?.Navigate(typeof(ProductPage));
        }

        private void ViewStaff_Click(object sender, RoutedEventArgs e)
        {
            if (Account.CurrentUserType != 1)
            {
                ContentDialog dialog = new ContentDialog
                {
                    Title = "Không có quyền",
                    Content = "Bạn không có quyền truy cập trang quản lý nhân viên.",
                    CloseButtonText = "Đóng",
                    XamlRoot = this.XamlRoot
                };
                _ = dialog.ShowAsync();
                return;
            }
            (this.Parent as Frame)?.Navigate(typeof(StaffPage));
        }

        private void ViewCustomers_Click(object sender, RoutedEventArgs e)
        {
            if (Account.CurrentUserType != 1)
            {
                ContentDialog dialog = new ContentDialog
                {
                    Title = "Không có quyền",
                    Content = "Bạn không có quyền truy cập trang quản lý nhân viên.",
                    CloseButtonText = "Đóng",
                    XamlRoot = this.XamlRoot
                };
                _ = dialog.ShowAsync();
                return;
            }
            (this.Parent as Frame)?.Navigate(typeof(CustomersPage));
        }

        private void ViewPayments_Click(object sender, RoutedEventArgs e)
        {
            (this.Parent as Frame)?.Navigate(typeof(PaymentMethodPage));
        }

        private void ViewBills_Click(object sender, RoutedEventArgs e)
        {
            (this.Parent as Frame)?.Navigate(typeof(BillManage));
        }

        private void NavigateSafely(Type pageType)
        {
            var parentFrame = this.Parent as Frame;
            if (parentFrame != null)
            {
                parentFrame.Navigate(pageType);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Khong tim thay Frame de navigate.");
            }
        }
    }
}