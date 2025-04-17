using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using CafePOS.DAO;
using CafePOS.DTO;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Protection.PlayReady;

namespace CafePOS.Views
{
    public sealed partial class BillManage : Page
    {
        private int currentPage = 1;
        private int pageSize = 3;
        private int totalPages = 1;
        public List<Bill> Bills { get; set; } = new List<Bill>();
        public List<FullBill> FullBills { get; set; } = new List<FullBill>();

        public BillManage()
        {
            this.InitializeComponent();
            LoadDefaultBills();
        }

        private async void LoadDefaultBills()
        {
            DateTime today = DateTime.Today;
            CheckInDatePicker.Date = today;
            CheckOutDatePicker.Date = today;
            await LoadFullBillsByDate(today, today);
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (CheckInDatePicker.Date == null || CheckOutDatePicker.Date == null)
            {
                Debug.WriteLine("Please select valid dates.");
                return;
            }

            DateTime checkIn = CheckInDatePicker.Date.DateTime;
            DateTime checkOut = CheckOutDatePicker.Date.DateTime;
            await LoadFullBillsByDate(checkIn, checkOut);
        }
        private async Task LoadBillsByDate(DateTime checkIn, DateTime checkOut)
        {
            var client = DataProvider.Instance.Client;
            var response = await client.GetAllBills.ExecuteAsync();

            if (response.Data?.AllBills?.Edges == null)
            {
                Debug.WriteLine("Error: No data received from GraphQL.");
                return;
            }

            var allBills = response.Data.AllBills.Edges
                .Select(edge => new Bill(
                    edge.Node.Id,
                    DateTime.TryParse(edge.Node.DateCheckIn, out DateTime checkIn) ? checkIn : (DateTime?)null,
                    DateTime.TryParse(edge.Node.DateCheckOut, out DateTime checkOut) ? checkOut : (DateTime?)null,
                    edge.Node.Status
                ))
                .ToList();

            var filteredBills = allBills
                .Where(bill => bill.DateCheckIn.HasValue &&
                               bill.DateCheckIn.Value.Date >= checkIn.Date &&
                               bill.DateCheckIn.Value.Date <= checkOut.Date)
                .ToList();

            if (filteredBills.Any())
            {
                Bills = filteredBills;
                BillListView.ItemsSource = Bills;
            }
            else
            {
                Debug.WriteLine("No bills found for the selected date range.");
                Bills.Clear();
                BillListView.ItemsSource = null;
            }
        }

        private async void CheckOutButton_Click(object sender, RoutedEventArgs e)
        {
            if (BillListView.SelectedItem is Bill selectedBill)
            {
                bool success = await BillDAO.Instance.CheckOutAsync(selectedBill.Id, 0, 0);
                if (success)
                {
                    Debug.WriteLine($"Bill {selectedBill.Id} checked out successfully.");
                    await LoadBillsByDate(CheckInDatePicker.Date.DateTime, CheckOutDatePicker.Date.DateTime);
                }
                else
                {
                    Debug.WriteLine("Checkout failed.");
                }
            }
        }

        private async Task LoadFullBillsByDate(DateTime checkIn, DateTime checkOut)
        {
            var client = DataProvider.Instance.Client;
            var response = await client.GetAllFullBills.ExecuteAsync();

            if (response.Data?.AllBills?.Edges == null)
            {
                Debug.WriteLine("Error: No data received from GraphQL (FullBills).");
                return;
            }

            // ✅ Keep raw FullBill objects
            FullBills = response.Data.AllBills.Edges
                .Select(edge => new FullBill(edge.Node))
                .Where(bill => bill.DateCheckIn.HasValue &&
                               bill.DateCheckIn.Value.Date >= checkIn.Date &&
                               bill.DateCheckIn.Value.Date <= checkOut.Date)
                .ToList();

            currentPage = 1;
            DisplayPagedBills(FullBills);
        }

        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage < totalPages)
            {
                currentPage++;
                DisplayPagedBills(FullBills);
            }
        }

        private void PreviousPage_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage > 1)
            {
                currentPage--;
                DisplayPagedBills(FullBills);
            }
        }
        private void DisplayPagedBills(List<FullBill> list)
        {
            totalPages = (int)Math.Ceiling((double)list.Count / pageSize);

            var pageData = list
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .Select(bill => new
                {
                    bill.Id,
                    bill.IdTable,
                    DateCheckIn = bill.DateCheckIn?.ToString("dd/MM/yyyy HH:mm"),
                    DateCheckOut = bill.DateCheckOut?.ToString("dd/MM/yyyy HH:mm"),
                    StatusText = bill.Status == 1 ? "Đã thanh toán" : "Chưa thanh toán",
                    TotalAmount = bill.TotalAmount.ToString("C0", CultureInfo.GetCultureInfo("vi-VN")),
                    Discount = $"{bill.Discount:0}%",
                    FinalAmount = bill.FinalAmount.ToString("C0", CultureInfo.GetCultureInfo("vi-VN")),
                    TableLabel = $"Bàn {bill.IdTable}",
                    bill.PaymentMethod
                })
                .ToList();

            BillListView.ItemsSource = pageData;
            PageIndicator.Text = $"Page {currentPage} of {totalPages}";
        }
    }
}
