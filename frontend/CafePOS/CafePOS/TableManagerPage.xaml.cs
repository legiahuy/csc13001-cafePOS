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


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace CafePOS
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TableManagerPage : Page
    {
        public TableManagerPage()
        {
            this.InitializeComponent();
            LoadTable();
            LoadCategory();
        }

        private async void LoadCategory()
        {
            List<Category> listCategory = await CategoryDAO.Instance.GetListCategoryAsync();
            CategoryComboBox.ItemsSource = listCategory;
            CategoryComboBox.DisplayMemberPath = "Name";
        }
        private async Task LoadDrinkListByCategoryID(int id)
        {
            List<Drink> listDrink = await DrinkDAO.Instance.GetDrinkByCategoryIDAsync(id);
            FoodComboBox.ItemsSource = listDrink;
            FoodComboBox.DisplayMemberPath = "Name";
        }

        private async void LoadTable()
        {

            List<CafeTable> tableList = await CafeTableDAO.Instance.GetAllCafeTablesAsync();

            foreach (var table in tableList)
            {
                int billId = await BillDAO.Instance.GetUncheckBillIDByTableID(table.Id, 0);
                table.Status = (billId != -1) ? "Có Khách" : "Trống";
            }
            TableItemsControl.ItemsSource = tableList; // Let XAML handle the UI updates
        }

        //void btn_Click(object sender, RoutedEventArgs e)
        //{
        //    int tableID = ((sender as Button).Tag as CafeTable)!.Id;
        //    ShowBill(tableID);
        //}

        async void ShowBill(int tableId)
        {
            List<Menu> listBillInfo = await MenuDAO.Instance.GetListMenuByTableAsync(tableId);
            OrderListView.ItemsSource = listBillInfo;
            double totalPrice = 0;
            foreach (Menu item in listBillInfo)
            {
                totalPrice += item.TotalPrice;
            }
            TotalPriceTextBlock.Text = totalPrice.ToString("C0", new System.Globalization.CultureInfo("vi-VN"));
        }

        private async void CategoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CategoryComboBox.SelectedItem is Category selectedCategory)
            {
                await LoadDrinkListByCategoryID(selectedCategory.ID);
            }
        }
        private async void btnAddFood_Click(object sender, RoutedEventArgs e)
        {
            if (OrderListView.Tag is not CafeTable table)
            {
                await ShowDialog("Error", "Please select a table first.");
                Debug.WriteLine("Table selection error: OrderListView.Tag is null or incorrect.");
                return;
            }

            Debug.WriteLine($"Selected Table ID: {table.Id}, Name: {table.Name}");

            int idBill = await BillDAO.Instance.GetUncheckBillIDByTableID(table.Id, 0);
            int foodID = (FoodComboBox.SelectedItem as Drink)?.ID ?? -1;
            int count = (int)QuantityBox.Value;

            if (foodID == -1)
            {
                await ShowDialog("Error", "Please select a valid food item.");
                return;
            }

            float unitPrice = GetUnitPrice(foodID);
            float totalPrice = unitPrice * count;

            if (idBill == -1)
            {
                int newBillId = await BillDAO.Instance.InsertBillAsync(table.Id);
                if (newBillId > 0)
                {
                    await BillInfoDAO.Instance.InsertBillInfoAsync(newBillId, foodID, count, unitPrice, totalPrice);
                }
            }
            else
            {
                await BillInfoDAO.Instance.InsertBillInfoAsync(idBill, foodID, count, unitPrice, totalPrice);
            }
            table.Status = "Có Khách";
            ShowBill(table.Id);
            LoadTable();
        }

        private async Task ShowDialog(string title, string content)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = title,
                Content = content,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
        }
        private float GetUnitPrice(int foodID)
        {
            return 10000f; 
        }
        private void TableItemsControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TableItemsControl.SelectedItem is CafeTable selectedTable)
            {
                Debug.WriteLine($"Table Selected: {selectedTable.Id}");
                ShowBill(selectedTable.Id);
            }
            else
            {
                Debug.WriteLine("Table selection changed, but no table found.");
            }
        }

        private void TableButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is CafeTable selectedTable)
            {
                Debug.WriteLine($"Button Clicked: {selectedTable}");
                OrderListView.Tag = selectedTable;
                ShowBill(selectedTable.Id);
            }
            else
            {
                Debug.WriteLine("Button Clicked: No table found in DataContext.");
            }
        }

        private async void btnCheckOut_Click(object sender, RoutedEventArgs e)
        {
            if (OrderListView.Tag is not CafeTable table)
            {
                await ShowDialog("Thông báo", "Vui lòng chọn bàn để thanh toán.");
                return;
            }

            int idBill = await BillDAO.Instance.GetUncheckBillIDByTableID(table.Id, 0);

            if (idBill == -1)
            {
                await ShowDialog("Thông báo", $"Không tìm thấy hóa đơn chưa thanh toán cho bàn {table.Name}.");
                return;
            }

            var dialogResult = await new ContentDialog
            {
                Title = "Xác nhận thanh toán",
                Content = $"Bạn có chắc chắn muốn thanh toán hóa đơn cho bàn {table.Name}?",
                PrimaryButtonText = "OK",
                CloseButtonText = "Hủy",
                XamlRoot = this.XamlRoot
            }.ShowAsync();

            if (dialogResult == ContentDialogResult.Primary)
            {
                bool success = await BillDAO.Instance.CheckOutAsync(idBill);

                if (success)
                {
                    OrderListView.ItemsSource = null;
                    OrderListView.Tag = null;
                    TotalPriceTextBlock.Text = "0 ₫";

                    table.Status = "Trống";
                    LoadTable(); 

                }
                else
                {
                    await ShowDialog("Lỗi", "Không thể cập nhật trạng thái hóa đơn.");
                }
            }
        }


    }
}
