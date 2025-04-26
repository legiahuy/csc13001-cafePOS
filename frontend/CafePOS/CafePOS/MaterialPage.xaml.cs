using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using CafePOS.DAO;
using CafePOS.DTO;
using CafePOS.Utilities;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Threading;
using System.Diagnostics;

namespace CafePOS
{
    public sealed partial class MaterialPage : Page
    {
        private string selectedImagePath = string.Empty;
        private List<Material> allMaterials = new List<Material>();

        public MaterialPage()
        {
            this.InitializeComponent();
            _ = InitializePageAsync();
            AddMaterialBinding();
            ClearFormFields();
        }

        private async Task InitializePageAsync()
        {
            await LoadMaterial();
        }

        private async void btnView_Click(object sender, RoutedEventArgs e)
        {
            await LoadMaterial();
        }

        private async void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (ProductListView.SelectedItem is Material selectedMaterial)
            {
                string name = ProductNameBox.Text?.Trim();
                int currentStock = (int)CurrentStockBox.Value;
                int minStock = (int)MinStockBox.Value;
                string unit = UnitBox.Text?.Trim();
                double price = (double)PriceBox.Value;
                string imagePath = string.IsNullOrEmpty(selectedImagePath) ? selectedMaterial.ImageUrl : selectedImagePath;

                // Validate empty data
                if (string.IsNullOrWhiteSpace(name))
                {
                    await DialogHelper.ShowErrorDialog("Lỗi", "Tên nguyên liệu không được để trống!", this.XamlRoot);
                    return;
                }

                if (string.IsNullOrWhiteSpace(unit))
                {
                    await DialogHelper.ShowErrorDialog("Lỗi", "Đơn vị tính không được để trống!", this.XamlRoot);
                    return;
                }

                // Validate value range
                if (currentStock < 0)
                {
                    await DialogHelper.ShowErrorDialog("Lỗi", "Số lượng tồn kho không được âm!", this.XamlRoot);
                    return;
                }

                if (minStock < 0)
                {
                    await DialogHelper.ShowErrorDialog("Lỗi", "Số lượng tồn kho tối thiểu không được âm!", this.XamlRoot);
                    return;
                }

                if (price <= 0)
                {
                    await DialogHelper.ShowErrorDialog("Lỗi", "Giá nguyên liệu phải lớn hơn 0!", this.XamlRoot);
                    return;
                }

                // Validate data consistency
                if (minStock > currentStock)
                {
                    await DialogHelper.ShowErrorDialog("Lỗi", "Số lượng tồn kho tối thiểu không được lớn hơn số lượng hiện tại!", this.XamlRoot);
                    return;
                }

                // Check for duplicate name (excluding current material)
                if (allMaterials.Any(m => m.Id != selectedMaterial.Id && m.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
                {
                    await DialogHelper.ShowErrorDialog("Lỗi", "Tên nguyên liệu đã tồn tại trong hệ thống!", this.XamlRoot);
                    return;
                }

                bool success = await MaterialDAO.Instance.UpdateMaterialAsync(
                    selectedMaterial.Id,
                    name,
                    currentStock,
                    minStock,
                    unit,
                    price,
                    imagePath);

                if (success)
                {
                    await LoadMaterial();
                    await DialogHelper.ShowSuccessDialog("Thành công", "Cập nhật nguyên liệu thành công!", this.XamlRoot);
                }
                else
                {
                    await DialogHelper.ShowErrorDialog("Lỗi", "Có lỗi khi cập nhật nguyên liệu. Vui lòng thử lại.", this.XamlRoot);
                }
            }
            else
            {
                await DialogHelper.ShowErrorDialog("Lỗi", "Vui lòng chọn một nguyên liệu.", this.XamlRoot);
            }
        }

        private async void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (ProductListView.SelectedItem is Material selectedMaterial)
            {
                ContentDialogResult result = await DialogHelper.ShowConfirmDialog(
                    "Xác nhận",
                    $"Bạn có chắc chắn muốn xóa nguyên liệu '{selectedMaterial.Name}' không?",
                    "Xóa",
                    "Hủy",
                    this.XamlRoot);

                if (result == ContentDialogResult.Primary)
                {
                    bool isDeleted = await MaterialDAO.Instance.DeleteMaterialAsync(selectedMaterial.Id);
                    Debug.WriteLine(selectedMaterial.Id);

                    if (isDeleted)
                    {
                        await LoadMaterial();
                        ClearFormFields();
                    }
                    else
                    {
                        await DialogHelper.ShowErrorDialog("Lỗi", "Không thể xóa nguyên liệu. Vui lòng thử lại!", this.XamlRoot);
                    }
                }
            }
            else
            {
                var errorDialog = new ContentDialog
                {
                    Title = "Lỗi",
                    Content = "Vui lòng chọn một nguyên liệu để xóa.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await errorDialog.ShowAsync();
            }
        }

        private async void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            string materialName = ProductNameBox.Text?.Trim();
            int currentStock = (int)CurrentStockBox.Value;
            int minStock = (int)MinStockBox.Value;
            string unit = UnitBox.Text?.Trim();
            double price = (double)PriceBox.Value;

            // Validate empty data
            if (string.IsNullOrWhiteSpace(materialName))
            {
                await DialogHelper.ShowErrorDialog("Lỗi", "Tên nguyên liệu không được để trống!", this.XamlRoot);
                return;
            }

            if (string.IsNullOrWhiteSpace(unit))
            {
                await DialogHelper.ShowErrorDialog("Lỗi", "Đơn vị tính không được để trống!", this.XamlRoot);
                return;
            }

            // Validate value range
            if (currentStock < 0)
            {
                await DialogHelper.ShowErrorDialog("Lỗi", "Số lượng tồn kho không được âm!", this.XamlRoot);
                return;
            }

            if (minStock < 0)
            {
                await DialogHelper.ShowErrorDialog("Lỗi", "Số lượng tồn kho tối thiểu không được âm!", this.XamlRoot);
                return;
            }

            if (price <= 0)
            {
                await DialogHelper.ShowErrorDialog("Lỗi", "Giá nguyên liệu phải lớn hơn 0!", this.XamlRoot);
                return;
            }

            // Validate data consistency
            if (minStock > currentStock)
            {
                await DialogHelper.ShowErrorDialog("Lỗi", "Số lượng tồn kho tối thiểu không được lớn hơn số lượng hiện tại!", this.XamlRoot);
                return;
            }

            // Check for duplicate name
            if (allMaterials.Any(m => m.Name.Equals(materialName, StringComparison.OrdinalIgnoreCase)))
            {
                await DialogHelper.ShowErrorDialog("Lỗi", "Tên nguyên liệu đã tồn tại trong hệ thống!", this.XamlRoot);
                return;
            }

            int isAdded = await MaterialDAO.Instance.InsertMaterialAsync(
                materialName,
                currentStock,
                minStock,
                unit,
                price,
                selectedImagePath);

            if (isAdded != -1)
            {
                await DialogHelper.ShowSuccessDialog("Thành công", "Nguyên liệu đã được thêm thành công!", this.XamlRoot);
                await LoadMaterial();
                ClearFormFields();
            }
            else
            {
                await DialogHelper.ShowErrorDialog("Lỗi", "Có lỗi khi thêm nguyên liệu vào cơ sở dữ liệu. Vui lòng thử lại.", this.XamlRoot);
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearFormFields();
            ProductListView.SelectedItem = null;
        }

        private void ClearFormFields()
        {
            ProductIDBox.Text = "";
            ProductNameBox.Text = "";
            CurrentStockBox.Value = 0;
            MinStockBox.Value = 0;
            UnitBox.Text = "";
            PriceBox.Value = 0;
            selectedImagePath = string.Empty;
            MaterialImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/default-image.jpg"));
        }

        public async Task LoadMaterial()
        {
            allMaterials = await MaterialDAO.Instance.GetListMaterialAsync();
            ProductListView.ItemsSource = allMaterials;
        }

        void AddMaterialBinding()
        {
            var productIDBinding = new Binding
            {
                Path = new PropertyPath("Id"),
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

            var currentStockBinding = new Binding
            {
                Path = new PropertyPath("CurrentStock"),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };
            CurrentStockBox.SetBinding(NumberBox.ValueProperty, currentStockBinding);

            var minStockBinding = new Binding
            {
                Path = new PropertyPath("MinStock"),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };
            MinStockBox.SetBinding(NumberBox.ValueProperty, minStockBinding);

            var unitBinding = new Binding
            {
                Path = new PropertyPath("Unit"),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };
            UnitBox.SetBinding(TextBox.TextProperty, unitBinding);

            var priceBinding = new Binding
            {
                Path = new PropertyPath("Price"),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };
            PriceBox.SetBinding(NumberBox.ValueProperty, priceBinding);
        }

        private void ProductListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedMaterial = (Material)ProductListView.SelectedItem;

            if (selectedMaterial != null)
            {
                this.DataContext = selectedMaterial;
                ProductIDBox.Text = selectedMaterial.Id.ToString();
                Debug.WriteLine(selectedMaterial.ImageUrl);

                // Cập nhật hình ảnh
                if (!string.IsNullOrEmpty(selectedMaterial.ImageUrl) || selectedMaterial.ImageUrl.Equals(""))
                {
                    try
                    {
                        MaterialImage.Source = new BitmapImage(new Uri("ms-appx://" + selectedMaterial.ImageUrl));
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

                // Reset selectedImagePath when selecting a new material
                selectedImagePath = string.Empty;
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
                    StorageFolder materialFolder = await assetsFolder.GetFolderAsync("Material");

                    StorageFile copiedFile = await file.CopyAsync(materialFolder, file.Name, NameCollisionOption.GenerateUniqueName);

                    selectedImagePath = $"/Assets/Material/{copiedFile.Name}";

                    BitmapImage bitmapImage = new BitmapImage();
                    using (var stream = await file.OpenReadAsync())
                    {
                        await bitmapImage.SetSourceAsync(stream);
                    }
                    MaterialImage.Source = bitmapImage;
                }
                catch (Exception ex)
                {
                    await DialogHelper.ShowErrorDialog("Lỗi", $"Không thể sao chép tệp hình ảnh: {ex.Message}", this.XamlRoot);
                }
            }
        }
    }
}