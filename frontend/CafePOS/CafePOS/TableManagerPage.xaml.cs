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
using CafePOS.Helpers;

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
            _ = LoadTable(); // Gọi không chờ
            LoadCategory();
            _ = LoadWeatherRecommendations();
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

        private async Task LoadTable()
        {
            List<CafeTable> tableList = await CafeTableDAO.Instance.GetAllCafeTablesAsync();

            foreach (var table in tableList)
            {
                int billId = await BillDAO.Instance.GetUncheckBillIDByTableID(table.Id, 0);
                table.Status = (billId != -1) ? "Có khách" : "Trống";

                Debug.WriteLine($"[LoadTable] {table.Name} - BillID: {billId} => Status: {table.Status}");
            }

            TableItemsControl.ItemsSource = tableList;
            MoveTableComboBox.ItemsSource = tableList;
            MoveTableComboBox.DisplayMemberPath = "Name";

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
                await ShowDialog("Lỗi", "Vui lòng chọn một bàn trước.");
                Debug.WriteLine("Lỗi chọn bàn: OrderListView.Tag là null hoặc không đúng.");
                return;
            }

            Debug.WriteLine($"Bàn được chọn ID: {table.Id}, Tên: {table.Name}");

            int idBill = await BillDAO.Instance.GetUncheckBillIDByTableID(table.Id, 0);
            int foodID = (FoodComboBox.SelectedItem as Drink)?.ID ?? -1;
            int count = (int)QuantityBox.Value;

            if (foodID == -1)
            {
                await ShowDialog("Lỗi", "Vui lòng chọn món ăn hợp lệ.");
                return;
            }

            float unitPrice = GetUnitPrice(foodID);
            float totalPrice = unitPrice * count;

            if (idBill == -1)
            {
                int newBillId = await BillDAO.Instance.InsertBillAsync(table.Id, Account.CurrentUserStaffId);
                Debug.WriteLine("ADD NEW BILL", newBillId);
                if (newBillId > 0)
                {
                    await BillInfoDAO.Instance.InsertBillInfoAsync(newBillId, foodID, count, unitPrice, totalPrice);
                }
            }
            else
            {
                await BillInfoDAO.Instance.InsertBillInfoAsync(idBill, foodID, count, unitPrice, totalPrice);
            }

            ShowBill(table.Id);
            await LoadTable();
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
            var selectedDrink = FoodComboBox.SelectedItem as Drink;
            return selectedDrink?.Price ?? 0f;
        }
        private void TableItemsControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TableItemsControl.SelectedItem is CafeTable selectedTable)
            {
                Debug.WriteLine($"Bàn được chọn: {selectedTable.Id}");
                ShowBill(selectedTable.Id);
            }
            else
            {
                Debug.WriteLine("Thay đổi lựa chọn bàn, nhưng không tìm thấy bàn.");
            }
        }

        private void TableButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is CafeTable selectedTable)
            {
                Debug.WriteLine($"Nút đã được nhấn: {selectedTable}");
                OrderListView.Tag = selectedTable;
                ShowBill(selectedTable.Id);
            }
            else
            {
                Debug.WriteLine("Nút đã được nhấn: Không tìm thấy bàn trong DataContext.");
            }
        }


        //private async void btnCheckOut_Click(object sender, RoutedEventArgs e)
        //{
        //    if (OrderListView.Tag is not CafeTable table)
        //    {
        //        await ShowDialog("Thông báo", "Vui lòng chọn bàn để thanh toán.");
        //        return;
        //    }

        //    int idBill = await BillDAO.Instance.GetUncheckBillIDByTableID(table.Id, 0);
        //    if (idBill == -1)
        //    {
        //        await ShowDialog("Thông báo", $"Không tìm thấy hóa đơn chưa thanh toán cho bàn {table.Name}.");
        //        return;
        //    }

        //    float discount = (float)DiscountBox.Value;
        //    double originalTotal = 0;
        //    if (OrderListView.ItemsSource is IEnumerable<Menu> items)
        //    {
        //        originalTotal = items.Sum(item => item.TotalPrice);
        //    }
        //    double finalTotal = originalTotal * (100 - discount) / 100;
        //    var culture = new System.Globalization.CultureInfo("vi-VN");
        //    string originalStr = originalTotal.ToString("C0", culture);
        //    string finalStr = finalTotal.ToString("C0", culture);

        //    string content = $"Bạn có chắc chắn muốn thanh toán hóa đơn cho bàn {table.Name}?\n" +
        //                     $"Tổng tiền: {originalStr}\n" +
        //                     $"Giảm giá: {discount}%\n" +
        //                     $"Thanh toán: {finalStr}";

        //    var dialogResult = await new ContentDialog
        //    {
        //        Title = "Xác nhận thanh toán",
        //        Content = content,
        //        PrimaryButtonText = "OK",
        //        CloseButtonText = "Hủy",
        //        XamlRoot = this.XamlRoot
        //    }.ShowAsync();

        //    if (dialogResult == ContentDialogResult.Primary)
        //    {
        //        bool success = await BillDAO.Instance.CheckOutAsync(idBill, discount);

        //        if (success)
        //        {
        //            OrderListView.ItemsSource = null;
        //            OrderListView.Tag = null;
        //            TotalPriceTextBlock.Text = "0 ₫";
        //            DiscountBox.Value = 0;

        //            await LoadTable();

        //            await ShowDialog("Thành công", $"Đã thanh toán cho bàn {table.Name}.");
        //        }
        //        else
        //        {
        //            await ShowDialog("Lỗi", "Không thể cập nhật trạng thái hóa đơn.");
        //        }
        //    }
        //}
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

            // Lấy danh sách các món trong hóa đơn
            double originalTotal = 0;
            List<Menu> orderItems = new List<Menu>();
            if (OrderListView.ItemsSource is IEnumerable<Menu> items)
            {
                orderItems = items.ToList();
                originalTotal = items.Sum(item => item.TotalPrice);
            }

            // Tạo cửa sổ thanh toán mới
            var checkoutWindow = new CheckoutWindow();

            // Đăng ký sự kiện hoàn thành thanh toán
            checkoutWindow.CheckoutCompleted += async (s, success) =>
            {
                if (success)
                {
                    // Xóa thông tin đơn hàng hiện tại
                    OrderListView.ItemsSource = null;
                    OrderListView.Tag = null;
                    TotalPriceTextBlock.Text = "0 ₫";

                    // Cập nhật lại dữ liệu bàn
                    await LoadTable();
                }
            };

            // Khởi tạo dữ liệu cho cửa sổ thanh toán
            checkoutWindow.Initialize(table, idBill, orderItems, originalTotal);

            // Hiển thị cửa sổ thanh toán
            checkoutWindow.Activate();
        }
        private async void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (OrderListView.Tag is not CafeTable table)
            {
                await ShowDialog("Thông báo", "Vui lòng chọn bàn để hủy.");
                return;
            }

            int idBill = await BillDAO.Instance.GetUncheckBillIDByTableID(table.Id, 0);
            if (idBill == -1)
            {
                await ShowDialog("Thông báo", $"Không tìm thấy hóa đơn chưa thanh toán cho bàn {table.Name}.");
                return;
            }

            string content = $"Bạn có chắc chắn muốn hủy hóa đơn cho bàn {table.Name}?";

            var dialogResult = await new ContentDialog
            {
                Title = "Xác nhận hủy",
                Content = content,
                PrimaryButtonText = "OK",
                CloseButtonText = "Thoát",
                XamlRoot = this.XamlRoot
            }.ShowAsync();

            if (dialogResult == ContentDialogResult.Primary)
            {
                bool success = await BillDAO.Instance.CancelAsync(idBill);
                if (success)
                {
                    OrderListView.ItemsSource = null;
                    OrderListView.Tag = null;
                    TotalPriceTextBlock.Text = "0 ₫";

                    await LoadTable();

                    await ShowDialog("Thành công", $"Đã hủy thanh toán cho bàn {table.Name}.");
                }
                else
                {
                    await ShowDialog("Lỗi", "Không thể cập nhật trạng thái hóa đơn.");
                }
            }
        }
        private async void btnChangeTable_Click(object sender, RoutedEventArgs e)
        {
            if (OrderListView.Tag is not CafeTable currentTable)
            {
                await ShowDialog("Thông báo", "Vui lòng chọn bàn hiện tại để chuyển.");
                return;
            }

            if (MoveTableComboBox.SelectedItem is not CafeTable targetTable)
            {
                await ShowDialog("Thông báo", "Vui lòng chọn bàn đích.");
                return;
            }

            if (currentTable.Id == targetTable.Id)
            {
                await ShowDialog("Thông báo", "Không thể chuyển sang cùng một bàn.");
                return;
            }

            if (targetTable.Status == "Có khách")
            {
                await ShowDialog("Không thể chuyển bàn", $"Bàn {targetTable.Name} hiện đang có khách. Vui lòng chọn bàn trống.");
                return;
            }

            var confirmDialog = new ContentDialog
            {
                Title = "Xác nhận chuyển bàn",
                Content = $"Bạn có thật sự muốn chuyển bàn {currentTable.Name} → {targetTable.Name}?",
                PrimaryButtonText = "Chuyển",
                CloseButtonText = "Hủy",
                XamlRoot = this.XamlRoot
            };

            var result = await confirmDialog.ShowAsync();
            if (result != ContentDialogResult.Primary) return;

            int billId = await BillDAO.Instance.GetUncheckBillIDByTableID(currentTable.Id, 0);
            if (billId == -1)
            {
                await ShowDialog("Thông báo", $"Không có hóa đơn đang mở trên {currentTable.Name}.");
                return;
            }

            bool success = await BillDAO.Instance.ChangeTableAsync(billId, targetTable.Id);
            if (success)
            {
                await ShowDialog("Thành công", $"Đã chuyển hóa đơn từ {currentTable.Name} sang {targetTable.Name}.");
                await LoadTable();
                ShowBill(targetTable.Id);
                OrderListView.Tag = targetTable;
            }
            else
            {
                await ShowDialog("Lỗi", "Không thể chuyển bàn.");
            }
        }

        private async void CancelBill(int billId)
        {
            try
            {
                var result = await BillDAO.Instance.CancelAsync(billId);
                if (result)
                {
                    // Clear the current order view
                    OrderListView.ItemsSource = null;
                    OrderListView.Tag = null;
                    TotalPriceTextBlock.Text = "0 ₫";

                    // Reload the table status
                    await LoadTable();

                    await ShowDialog("Thành công", "Đã hủy hóa đơn thành công.");
                }
                else
                {
                    await ShowDialog("Lỗi", "Không thể hủy hóa đơn. Vui lòng thử lại.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in CancelBill: {ex.Message}");
                await ShowDialog("Lỗi", "Đã xảy ra lỗi khi hủy hóa đơn. Vui lòng thử lại.");
            }
        }
        private async Task LoadWeatherRecommendations()
        {
            try
            {
                var weather = await WeatherDAO.Instance.GetCurrentWeatherAsync();
                if (weather == null) return;

                string type = WeatherClassifier.ClassifyWeather(weather.Main, weather.TemperatureCelsius);

                var allDrinks = await DrinkDAO.Instance.GetListDrinkAsync();
                var allCategories = await CategoryDAO.Instance.GetListCategoryAsync();

                // Tạo Dictionary để lấy tên category theo ID
                var categoryMap = allCategories.ToDictionary(c => c.ID, c => c.Name);

                // Lọc đồ uống phù hợp với thời tiết dựa trên tên + category
                var recommendedDrinks = allDrinks
                    .Where(d =>
                    {
                        string categoryName = categoryMap.ContainsKey(d.CategoryId) ? categoryMap[d.CategoryId] : "";
                        string predictedType = WeatherDrinkClassifier.ClassifyDrinkByNameAndCategory(d.Name, categoryName);
                        return predictedType == type;
                    })
                    .ToList();

                WeatherDrinkComboBox.ItemsSource = recommendedDrinks;
                WeatherSummaryTextBlock.Text = $"Hôm nay thời tiết {WeatherClassifier.ToVietnamese(type)}, nhiệt độ {weather.TemperatureCelsius:0.#}°C. Gợi ý những món như sau:";
            }
            catch (Exception ex)
            {
                WeatherSummaryTextBlock.Text = "Không thể lấy thông tin thời tiết.";
            }
        }





    }
}
