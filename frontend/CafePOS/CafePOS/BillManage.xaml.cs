using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using CafePOS.DAO;
using CafePOS.DTO;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;

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
            // Load bills for today by default
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
            var bills = await BillDAO.Instance.GetBillsByDateAsync(checkIn, checkOut);
            if (bills != null && bills.Count > 0)
            {
                Bills = bills;
                BillListView.ItemsSource = Bills;
            }
            else
            {
                Debug.WriteLine("No bills found for the selected date range.");
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
