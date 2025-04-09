using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using CafePOS.Converter;
using CafePOS.DAO;
using CafePOS.DTO;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Pickers;
using Windows.Storage;
using System.Diagnostics;
using ClosedXML.Excel;

namespace CafePOS
{
    public sealed partial class ProductPage : Page
    {
        private string selectedImagePath = string.Empty;
        private List<Drink> allProducts = new List<Drink>(); 
        private List<Category> allCategories = new List<Category>();

        public ProductPage()
        {
            this.InitializeComponent();
            _ = InitializePageAsync();
            MaterialImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/default-image.jpg"));
        }

        private async Task InitializePageAsync()
        {
            await LoadProduct();
            await LoadAllCategories();
            LoadCategoryIntoCombobox(CategoryComboBox);
            LoadCategoryIntoCombobox(FilterCategoryComboBox);

            var allCategoriesItem = new Category { ID = -1, Name = "Tất cả danh mục" };
            var categories = new List<Category> { allCategoriesItem };
            categories.AddRange(allCategories);
            FilterCategoryComboBox.ItemsSource = categories;
            FilterCategoryComboBox.SelectedIndex = 0; // Select "All Categories" by default

            AddProductBinding();
        }

        private async Task LoadAllCategories()
        {
            allCategories = await CategoryDAO.Instance.GetListCategoryAsync();
        }

        private async void btnView_Click(object sender, RoutedEventArgs e)
        {
            await LoadProduct();
        }

        private async void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (ProductListView.SelectedItem is Drink selectedDrink)
            {
                string name = ProductNameBox.Text;
                string description = "Updated description";
                int categoryId = (int)CategoryComboBox.SelectedValue;
                int price = (int)PriceBox.Value;
                bool isAvailable = true; 
                string imagePath = string.IsNullOrEmpty(selectedImagePath) ? selectedDrink.ImageUrl : selectedImagePath;

                bool success = await DrinkDAO.Instance.UpdateProductAsync(selectedDrink.ID, name, description, categoryId, price, isAvailable, imagePath);

                if (success)
                {
                    await LoadProduct(); 
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
                    var errorDialog = new ContentDialog
                    {
                        Title = "Lỗi",
                        Content = "Có lỗi khi chỉnh sửa món. Vui lòng thử lại.",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await errorDialog.ShowAsync();
                }
            }
            else
            {
                var errorDialog = new ContentDialog
                {
                    Title = "Lỗi",
                    Content = "Vui lòng chọn một món.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await errorDialog.ShowAsync();
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
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
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

            int isAdded = await DrinkDAO.Instance.InsertProductAsync(productName, categoryId.Value, price, selectedImagePath);

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
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await errorDialog.ShowAsync();
            }
        }

        public async Task LoadProduct()
        {
            allProducts = await DrinkDAO.Instance.GetListDrinkAsync();
            ApplyFilters(); 
        }

        private void ApplyFilters()
        {
            var filteredList = allProducts;

            if (FilterCategoryComboBox.SelectedItem is Category selectedCategory && selectedCategory.ID != -1)
            {
                filteredList = filteredList.Where(p => p.CategoryId == selectedCategory.ID).ToList();
            }

            string searchText = SearchBox.Text?.Trim().ToLower() ?? "";
            if (!string.IsNullOrEmpty(searchText))
            {
                filteredList = filteredList.Where(p =>
                    p.Name.ToLower().Contains(searchText)).ToList();
            }

            ProductListView.ItemsSource = filteredList;
        }

        void AddProductBinding()
        {
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
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit,
                Converter = new PriceToCurrencyConverter()
            };
            PriceBox.SetBinding(NumberBox.ValueProperty, priceBinding);
        }

        private void LoadCategoryIntoCombobox(ComboBox cb)
        {
            cb.ItemsSource = allCategories;
            cb.DisplayMemberPath = "Name";
            cb.SelectedValuePath = "ID";
        }

        private void ProductListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedDrink = (Drink)ProductListView.SelectedItem;

            if (selectedDrink != null)
            {
                this.DataContext = selectedDrink;
                ProductIDBox.Text = selectedDrink.ID.ToString();
                ProductNameBox.Text = selectedDrink.Name;
                PriceBox.Value = selectedDrink.Price;

                _ = SetCategoryComboBoxAsync(selectedDrink.CategoryId);
            }
        }

        private async Task SetCategoryComboBoxAsync(int categoryId)
        {
            var category = await CategoryDAO.Instance.GetCategoryByIdAsync(categoryId);
            if (category != null && category.Count > 0)
            {
                CategoryComboBox.SelectedValue = category[0].ID;
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

                    this.DataContext = selectedDrink;
                    Debug.WriteLine(selectedDrink.ImageUrl);

                    if (!string.IsNullOrEmpty(selectedDrink.ImageUrl) || selectedDrink.ImageUrl.Equals(""))
                    {
                        try
                        {
                            MaterialImage.Source = new BitmapImage(new Uri("ms-appx://" + selectedDrink.ImageUrl));
                        }
                        catch
                        {
                            MaterialImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/default-image.jpg"));
                        }
                    }
                    else
                    {
                        MaterialImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/default-image.jpg"));
                    }

                    selectedImagePath = string.Empty;
                }
            }
        }

        private async void SelectImageButton_Click(object sender, RoutedEventArgs e)
        {
            var filePicker = new FileOpenPicker();
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainAppWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(filePicker, hwnd);
            filePicker.ViewMode = PickerViewMode.Thumbnail;
            filePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            filePicker.FileTypeFilter.Add(".jpg");
            filePicker.FileTypeFilter.Add(".jpeg");
            filePicker.FileTypeFilter.Add(".png");

            StorageFile file = await filePicker.PickSingleFileAsync();
            if (file != null)
            {
                string fullPath = file.Path;

                try
                {
                    StorageFolder appFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;

                    StorageFolder assetsFolder = await appFolder.GetFolderAsync("Assets");
                    StorageFolder productFolder = await assetsFolder.GetFolderAsync("Product");

                    StorageFile copiedFile = await file.CopyAsync(productFolder, file.Name, NameCollisionOption.GenerateUniqueName);

                    selectedImagePath = $"/Assets/Product/{copiedFile.Name}";

                    BitmapImage bitmapImage = new BitmapImage();
                    using (var stream = await file.OpenReadAsync())
                    {
                        await bitmapImage.SetSourceAsync(stream);
                    }
                    MaterialImage.Source = bitmapImage;
                }
                catch (Exception ex)
                {
                    ContentDialog dialog = new ContentDialog
                    {
                        Title = "Lỗi",
                        Content = $"Không thể sao chép tệp hình ảnh: {ex.Message}",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await dialog.ShowAsync();
                }
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void FilterCategoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private async void btnExportExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Hiển thị FileSavePicker để người dùng chọn vị trí lưu file
                var savePicker = new FileSavePicker();
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainAppWindow);
                WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);

                savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                savePicker.FileTypeChoices.Add("Excel Files", new List<string>() { ".xlsx" });
                savePicker.SuggestedFileName = "ProductList_" + DateTime.Now.ToString("yyyyMMdd");

                StorageFile file = await savePicker.PickSaveFileAsync();
                if (file != null)
                {
                    // Tạo workbook mới
                    using (var workbook = new XLWorkbook())
                    {
                        // Thêm worksheet với tên "Products"
                        var worksheet = workbook.Worksheets.Add("Products");

                        // Thêm header
                        worksheet.Cell(1, 1).Value = "ID";
                        worksheet.Cell(1, 2).Value = "Tên sản phẩm";
                        worksheet.Cell(1, 3).Value = "Danh mục";
                        worksheet.Cell(1, 4).Value = "Giá";

                        // Format header
                        var headerRange = worksheet.Range(1, 1, 1, 4);
                        headerRange.Style.Font.Bold = true;
                        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

                        // Lấy danh sách sản phẩm hiện tại (có thể là allProducts hoặc ProductListView.ItemsSource)
                        var products = (List<Drink>)ProductListView.ItemsSource;

                        // Thêm dữ liệu
                        int row = 2;
                        foreach (var product in products)
                        {
                            worksheet.Cell(row, 1).Value = product.ID;
                            worksheet.Cell(row, 2).Value = product.Name;

                            // Lấy tên danh mục
                            string categoryName = "Unknown";
                            var category = allCategories.FirstOrDefault(c => c.ID == product.CategoryId);
                            if (category != null)
                            {
                                categoryName = category.Name;
                            }
                            worksheet.Cell(row, 3).Value = categoryName;

                            // Format giá thành tiền tệ
                            worksheet.Cell(row, 4).Value = product.Price;
                            worksheet.Cell(row, 4).Style.NumberFormat.Format = "#,##0";

                            row++;
                        }

                        // Auto-fit columns
                        worksheet.Columns().AdjustToContents();

                        // Lưu file
                        using (var stream = await file.OpenStreamForWriteAsync())
                        {
                            workbook.SaveAs(stream);
                        }

                        // Thông báo thành công
                        var successDialog = new ContentDialog
                        {
                            Title = "Thành công",
                            Content = "Danh sách sản phẩm đã được xuất thành công!",
                            CloseButtonText = "OK",
                            XamlRoot = this.XamlRoot
                        };
                        await successDialog.ShowAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                // Xử lý lỗi
                var errorDialog = new ContentDialog
                {
                    Title = "Lỗi",
                    Content = $"Có lỗi khi xuất file Excel: {ex.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await errorDialog.ShowAsync();
            }
        }
    }
}