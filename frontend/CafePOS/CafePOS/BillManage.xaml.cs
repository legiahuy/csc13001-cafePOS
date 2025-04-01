using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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
        public List<Bill> Bills { get; set; } = new List<Bill>();

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
            await LoadBillsByDate(today, today);
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
            await LoadBillsByDate(checkIn, checkOut);
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
    }
}
