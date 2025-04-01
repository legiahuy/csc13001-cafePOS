using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using CafePOS.DAO;
using CafePOS.DTO;
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
    public sealed partial class InventoryPage : Page
    {
        public InventoryPage()
        {
            this.InitializeComponent();
            _ = LoadProduct();
            LoadCategoryIntoCombobox(CategoryComboBox);
            AddProductBinding();
        }

        private async void btnView_Click(object sender, RoutedEventArgs e) {
            await LoadProduct();
        }

        private async void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (ProductListView.SelectedItem is Drink selectedDrink)
            {
                string name = ProductNameBox.Text;
                string description = "Updated description"; // Add a proper description field in UI if needed
                int categoryId = (int)CategoryComboBox.SelectedValue;
                int price = (int)PriceBox.Value;
                bool isAvailable = true; // You might need a toggle for availability in UI

                bool success = await DrinkDAO.Instance.UpdateProductAsync(selectedDrink.ID, name, description, categoryId, price, isAvailable);

                if (success)
                {
                    await LoadProduct(); // Refresh the list after updating
                    var successDialog = new ContentDialog
                    {
                        Title = "Thành công",
                        Content = "Sửa món thành công!",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await successDialog.ShowAsync();
                }
                else
                {
                    var successDialog = new ContentDialog
                    {
                        Title = "Lỗi",
                        Content = "Có lỗi khi chỉnh sửa món. Vui lòng thử lại.",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await successDialog.ShowAsync();
                }
            }
            else
            {
                var successDialog = new ContentDialog
                {
                    Title = "Lỗi",
                    Content = "Vui lòng chọn một món.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await successDialog.ShowAsync();
            }
        }

        private async void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (ProductListView.SelectedItem is Drink selectedDrink)
            {
                bool isDeleted = await DrinkDAO.Instance.DeleteProductAsync(selectedDrink.ID);

                if (isDeleted)
                {
                    await LoadProduct();
                    ProductIDBox.Text = "";
                    ProductNameBox.Text = "";
                    CategoryComboBox.SelectedIndex = -1;
                    PriceBox.Value = 0;
                }
                else
                {
                    ContentDialog dialog = new ContentDialog
                    {
                        Title = "Lỗi",
                        Content = "Không thể xóa sản phẩm. Vui lòng thử lại!",
                        CloseButtonText = "OK"
                    };
                    await dialog.ShowAsync();
                }
            }
        }


        private async void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            string productName = ProductNameBox.Text;
            int? categoryId = (int?)CategoryComboBox.SelectedValue; 
            int price = (int)PriceBox.Value;

            if (string.IsNullOrWhiteSpace(productName) || categoryId == null || price <= 0)
            {
                var dialog = new ContentDialog
                {
                    Title = "Lỗi",
                    Content = "Vui lòng điền đầy đủ thông tin và chọn danh mục hợp lệ!",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await dialog.ShowAsync();
                return;
            }

            int isAdded = await DrinkDAO.Instance.InsertProductAsync(productName, categoryId.Value, price);

            if (isAdded != -1)
            {
                var successDialog = new ContentDialog
                {
                    Title = "Thành công",
                    Content = "Món đã được thêm thành công!",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await successDialog.ShowAsync();
                

                await LoadProduct();
            }
            else
            {
                var errorDialog = new ContentDialog
                {
                    Title = "Lỗi",
                    Content = "Có lỗi khi thêm món vào cơ sở dữ liệu. Vui lòng thử lại.",
                    CloseButtonText = "OK"
                };
                await errorDialog.ShowAsync();
            }
        }

        public async Task LoadProduct()
        {
            List<Drink> productList = await DrinkDAO.Instance.GetListDrinkAsync();
            ProductListView.ItemsSource = productList;
        }

        void AddProductBinding() {
            var productIDBinding = new Binding
            {
                Path = new PropertyPath("ID"),
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };
            ProductIDBox.SetBinding(TextBox.TextProperty, productIDBinding);

            var productNameBinding = new Binding
            {
                Path = new PropertyPath("Name"),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };
            ProductNameBox.SetBinding(TextBox.TextProperty, productNameBinding);

            var priceBinding = new Binding
            {
                Path = new PropertyPath("Price"),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };
            PriceBox.SetBinding(NumberBox.ValueProperty, priceBinding);
        }

        private async void LoadCategoryIntoCombobox(ComboBox cb)
        {
            var categories = await CategoryDAO.Instance.GetListCategoryAsync();

            cb.ItemsSource = categories;
            cb.DisplayMemberPath = "Name"; 
            cb.SelectedValuePath = "ID"; 
        }

        private void ProductListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedDrink = (Drink)ProductListView.SelectedItem;

            if (selectedDrink != null)
            {
                this.DataContext = selectedDrink;
            }
        }

        private async void ProductNameBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var selectedDrink = (Drink)this.DataContext;

            if (selectedDrink != null)
            {
                var category = await CategoryDAO.Instance.GetCategoryByIdAsync(selectedDrink.CategoryId);

                if (category != null)
                {
                    CategoryComboBox.SelectedValue = category[0].ID;
                }
            }
        }
    }
}