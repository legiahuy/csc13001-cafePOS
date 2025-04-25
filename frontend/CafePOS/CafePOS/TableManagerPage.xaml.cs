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
using CafePOS.Services;
using CafePOS.GraphQL;
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
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using StrawberryShake;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace CafePOS
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TableManagerPage : Page, INotifyPropertyChanged
    {
        private readonly IWeatherService _weatherService;
        private readonly IGeminiService _geminiService;
        private readonly ICafePOSClient _cafePOSClient;

        public ObservableCollection<ProductDTO> RecommendedProducts { get; } = new();
        public ObservableCollection<ProductDTO> Recommendations { get; private set; }
        
        private string _currentTemperature;
        public string CurrentTemperature
        {
            get => _currentTemperature;
            set
            {
                _currentTemperature = value;
                DispatcherQueue.TryEnqueue(() =>
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentTemperature)));
                });
            }
        }

        private string _aiRecommendation;
        public string AiRecommendation
        {
            get => _aiRecommendation;
            set
            {
                _aiRecommendation = value;
                DispatcherQueue.TryEnqueue(() =>
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AiRecommendation)));
                });
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        public TableManagerPage()
        {
            this.InitializeComponent();
            var app = Application.Current as App;
            _weatherService = app.ServiceProvider.GetRequiredService<IWeatherService>();
            _geminiService = app.ServiceProvider.GetRequiredService<IGeminiService>();
            _cafePOSClient = app.ServiceProvider.GetRequiredService<ICafePOSClient>();
            Recommendations = new ObservableCollection<ProductDTO>();
            DataContext = this;
            _ = LoadTable(); // Gọi không chờ
            LoadCategory();
            LoadRecommendations();
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
                CloseButtonText = "Đóng",
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

        private async void LoadRecommendations()
        {
            try
            {
                Recommendations.Clear(); 
                RecommendedProducts.Clear(); 
                AiRecommendation = "Đang lấy gợi ý từ AI..."; 

                // Fetch real temperature directly
                double temperature = await _weatherService.GetCurrentTemperatureAsync();
                Debug.WriteLine($"[REAL WEATHER] Fetched temperature: {temperature}°C");
                
                CurrentTemperature = $"{temperature:F1}°C"; 

                // Fetch ALL products using GraphQL client
                IOperationResult<IGetAllProductsResult> allProductsResult = 
                    await _cafePOSClient.GetAllProducts.ExecuteAsync();

                // Check for general operation errors (network, etc.)
                if (allProductsResult.Errors.Any())
                {
                    // ... (error handling as before) ...
                    return;
                }

                // Safely access data and check for GraphQL-specific errors
                List<ProductDTO> allProducts = new List<ProductDTO>();
                var productEdges = allProductsResult.Data?.AllProducts?.Edges;

                if (productEdges != null)
                {
                    allProducts = productEdges
                        .Where(edge => edge?.Node != null) // Ensure edge and node aren't null
                        .Select(edge => edge.Node) // Select the Node from the Edge
                        .Select(p => new ProductDTO
                        {
                            Id = p.Id,
                            Name = p.Name ?? "N/A", 
                            Description = p.Description ?? "",
                            Price = p.Price,
                            ImageUrl = p.ImageUrl ?? "",
                            // Add other relevant properties if needed from IGetAllProducts_AllProducts_Edges_Node
                        }).ToList();
                    Debug.WriteLine($"Fetched {allProducts.Count} products.");
                }
                else
                {
                    Debug.WriteLine("GraphQL Data, AllProducts, or Edges were null.");
                }

                if (!allProducts.Any())
                {
                     // ... (error handling as before) ...
                     return;
                }

                // Get AI recommendations using Gemini with ALL product names
                var allProductNames = allProducts.Select(p => p.Name).ToList();

                // Debugging before API call (removed simulation flag)
                Debug.WriteLine($"--- Calling Gemini API ---");
                Debug.WriteLine($"Temperature Sent: {temperature}°C");
                // Debug.WriteLine($"Products Sent ({allProductNames.Count}): {string.Join(", ", allProductNames)}"); 

                string aiSuggestionText = "Error: Service call did not complete."; 
                try
                {
                    Debug.WriteLine("[LoadRecommendations] Calling _geminiService.GetDrinkRecommendationsAsync...");
                    aiSuggestionText = await _geminiService.GetDrinkRecommendationsAsync(temperature, allProductNames); 
                    Debug.WriteLine("[LoadRecommendations] Call to _geminiService.GetDrinkRecommendationsAsync completed.");
                    Debug.WriteLine($"Gemini Raw Response Text received in Page: {aiSuggestionText}");
                }
                catch(Exception serviceEx)
                {
                     // ... (error handling as before) ...
                }


                // Construct the display message using the real temperature
                string displayMessage = $"Với thời tiết {CurrentTemperature}, gợi ý cho bạn:"; 
                if (temperature < 18) // Using the actual temperature now
                {
                    displayMessage = $"Với thời tiết lạnh {CurrentTemperature}, gợi ý đồ uống ấm nóng cho bạn:";
                }
                else if (temperature > 28) // Using the actual temperature now
                {
                     displayMessage = $"Với thời tiết nóng {CurrentTemperature}, gợi ý đồ uống mát lạnh cho bạn:";
                }
                AiRecommendation = displayMessage;

                // --- Populate the Recommendations ListView based on AI suggestion text --- 
                Recommendations.Clear(); 
                var suggestedNames = aiSuggestionText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                                                  .Select(name => name.Trim())
                                                  .Where(name => !string.IsNullOrWhiteSpace(name))
                                                  .ToList();
                
                Debug.WriteLine($"AI suggested names ({suggestedNames.Count}): {string.Join(", ", suggestedNames)}");

                foreach (var suggestedName in suggestedNames)
                {
                    var productToAdd = allProducts.FirstOrDefault(p => 
                        p.Name.Equals(suggestedName, StringComparison.OrdinalIgnoreCase) ||
                        p.Name.Equals(suggestedName.Replace("đá", "Đá"), StringComparison.OrdinalIgnoreCase) // Handle case variations like 'da' vs 'Đá'
                    );
                    
                    if (productToAdd != null)
                    {
                         if (!Recommendations.Any(r => r.Id == productToAdd.Id)) // Avoid duplicates
                         {
                             Recommendations.Add(productToAdd);
                         }
                    }
                    else
                    {
                        Debug.WriteLine($"AI suggested product not found in master list: '{suggestedName}'");
                    }
                }
                 Debug.WriteLine($"Populated Recommendations list with {Recommendations.Count} items based on AI suggestion.");
                // --- End of populating ListView --- 
            }
            catch (Exception ex) 
            {
                 // ... (outer error handling as before) ...
            }
        }

        private void AddRecommendedItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button addButton && addButton.Tag is ProductDTO product)
            {
                // Try finding the ListViewItem container first
                var listViewItem = FindParent<ListViewItem>(addButton);

                if (listViewItem != null)
                {
                    // Now search for the NumberBox within this specific ListViewItem
                    var quantityBox = FindChild<NumberBox>(listViewItem, "RecommendedQuantityBox");
                    if (quantityBox != null)
                    {
                        int quantity = (int)quantityBox.Value;
                        if (quantity > 0)
                        {
                            AddFoodToOrder(product, quantity); // Pass quantity
                        }
                        else
                        {
                            _ = ShowDialog("Số lượng không hợp lệ", "Vui lòng nhập số lượng lớn hơn 0.");
                        }
                    }
                    else
                    {
                        Debug.WriteLine("Could not find RecommendedQuantityBox within ListViewItem");
                        _ = ShowDialog("Lỗi", "Không tìm thấy ô nhập số lượng (trong mục).");
                    }
                }
                else
                {
                     // Fallback: Try the previous method (searching up to Grid, then down)
                    var parentGrid = FindParent<Grid>(addButton);
                    if (parentGrid != null)
                    {
                        var quantityBox = FindChild<NumberBox>(parentGrid, "RecommendedQuantityBox");
                        if (quantityBox != null)
                        {
                             int quantity = (int)quantityBox.Value;
                             if (quantity > 0)
                             {
                                 AddFoodToOrder(product, quantity); // Pass quantity
                             }
                             else
                             {
                                 _ = ShowDialog("Số lượng không hợp lệ", "Vui lòng nhập số lượng lớn hơn 0.");
                             }
                        }
                        else
                        {
                             Debug.WriteLine("Could not find RecommendedQuantityBox via Grid fallback");
                             _ = ShowDialog("Lỗi", "Không tìm thấy ô nhập số lượng (dự phòng).");
                        }
                    }
                    else
                    {
                        Debug.WriteLine("Could not find parent ListViewItem or Grid for recommended item");
                        _ = ShowDialog("Lỗi", "Không thể xác định mục hoặc ô nhập số lượng.");
                    }
                }
            }
        }

        private async void AddFoodToOrder(ProductDTO product, int count = 1)
        {
            if (OrderListView.Tag is not CafeTable table)
            {
                await ShowDialog("Thông báo", "Vui lòng chọn bàn trước khi thêm món.");
                return;
            }

            if (count <= 0)
            {
                await ShowDialog("Số lượng không hợp lệ", "Số lượng phải lớn hơn 0.");
                return;
            }

            try
            {
                int idBill = await BillDAO.Instance.GetUncheckBillIDByTableID(table.Id, 0);
                float unitPrice = (float)product.Price;
                float totalPrice = unitPrice * count;

                if (idBill == -1)
                {
                    int newBillId = await BillDAO.Instance.InsertBillAsync(table.Id, Account.CurrentUserStaffId);
                     Debug.WriteLine($"Created new bill with ID: {newBillId} for table {table.Id}");
                    if (newBillId > 0)
                    {
                        await BillInfoDAO.Instance.InsertBillInfoAsync(newBillId, product.Id, count, unitPrice, totalPrice);
                         Debug.WriteLine($"Inserted {count} of product {product.Id} into new bill {newBillId}");
                    } else {
                         Debug.WriteLine($"Failed to create new bill for table {table.Id}");
                         await ShowDialog("Lỗi", "Không thể tạo hóa đơn mới.");
                         return; 
                    }
                    idBill = newBillId; 
                }
                else
                {
                    await BillInfoDAO.Instance.InsertBillInfoAsync(idBill, product.Id, count, unitPrice, totalPrice);
                    Debug.WriteLine($"Inserted {count} of product {product.Id} into existing bill {idBill}");
                }

                ShowBill(table.Id); 
                await LoadTable(); 
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in AddFoodToOrder: {ex}");
                await ShowDialog("Lỗi", $"Không thể thêm món: {ex.Message}");
            }
        }

        // +++ Add Visual Tree Helpers +++
        public static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null) return null;
            T parent = parentObject as T;
            if (parent != null)
                return parent;
            else
                return FindParent<T>(parentObject);
        }

        public static T FindChild<T>(DependencyObject parent, string childName) where T : DependencyObject
        {
            if (parent == null) return null;

            T foundChild = null;
            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);

            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                // Check if the current child IS the element we're looking for
                T childAsT = child as T;
                FrameworkElement childFrameworkElement = child as FrameworkElement;

                if (childAsT != null && childFrameworkElement != null && childFrameworkElement.Name == childName)
                {
                    return childAsT; // Found the specific named element of type T
                }
                else // If not, recurse into the current child's subtree
                {
                    foundChild = FindChild<T>(child, childName);
                    if (foundChild != null)
                    {
                        return foundChild; // Found it in a deeper level
                    }
                }
            }

            return null; // Not found anywhere in this parent's subtree
        }

    }
}
