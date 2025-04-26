using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using CafePOS.DAO;
using CafePOS.DTO;
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
using CafePOS.Utilities;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace CafePOS
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PaymentMethodPage : Page
    {
        private string selectedImagePath = string.Empty;
        private List<KeyValuePair<bool, string>> allStatus = new()
        {
            new(true, "Có"),
            new(false, "Không")
        };

        public PaymentMethodPage()
        {
            this.InitializeComponent();
            _ = LoadPaymentMethod();
            LoadStatusIntoCombobox(StatusComboBox);
            AddPaymentBinding();
            ClearFormFields();
        }

        private async Task<bool> ValidatePaymentMethodExists(int id)
        {
            var payments = await PaymentMethodDAO.Instance.GetAllPaymentMethodAsync();
            return payments.Any(p => p.Id == id);
        }

        private async Task<bool> ValidatePaymentNameExists(string name, int? excludeId = null)
        {
            var payments = await PaymentMethodDAO.Instance.GetAllPaymentMethodAsync();
            return payments.Any(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && p.Id != excludeId);
        }

        private async void btnView_Click(object sender, RoutedEventArgs e)
        {
            await LoadPaymentMethod();
        }

        private async void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (PaymentListView.SelectedItem is PaymentMethod selectedPayment)
            {
                string name = PaymentNameBox.Text.Trim();
                string imagePath = string.IsNullOrEmpty(selectedImagePath) ? selectedPayment.IconUrl! : selectedImagePath;
                string description = PaymentDescriptionBox.Text.Trim();
                bool isActive = (bool)StatusComboBox.SelectedValue;

                // Empty data validation
                if (string.IsNullOrWhiteSpace(name))
                {
                    await DialogHelper.ShowErrorDialog("Lỗi", "Vui lòng nhập tên phương thức thanh toán", this.XamlRoot);
                    return;
                }

                if (string.IsNullOrWhiteSpace(description))
                {
                    await DialogHelper.ShowErrorDialog("Lỗi", "Vui lòng nhập mô tả phương thức thanh toán", this.XamlRoot);
                    return;
                }

                // Data type validation
                if (!ValidationHelper.IsValidName(name))
                {
                    await DialogHelper.ShowErrorDialog("Lỗi", "Tên phương thức thanh toán không hợp lệ. Chỉ được chứa chữ cái, số và khoảng trắng", this.XamlRoot);
                    return;
                }

                if (!ValidationHelper.IsValidLength(description, 200))
                {
                    await DialogHelper.ShowErrorDialog("Lỗi", "Mô tả không được vượt quá 200 ký tự", this.XamlRoot);
                    return;
                }

                if (!await ValidationHelper.IsValidImagePath(imagePath))
                {
                    await DialogHelper.ShowErrorDialog("Lỗi", "Đường dẫn hình ảnh không hợp lệ", this.XamlRoot);
                    return;
                }

                // Data consistency validation
                if (!await ValidatePaymentMethodExists(selectedPayment.Id))
                {
                    await DialogHelper.ShowErrorDialog("Lỗi", "Không tìm thấy phương thức thanh toán", this.XamlRoot);
                    return;
                }

                if (await ValidatePaymentNameExists(name, selectedPayment.Id))
                {
                    await DialogHelper.ShowErrorDialog("Lỗi", "Tên phương thức thanh toán đã tồn tại", this.XamlRoot);
                    return;
                }

                bool success = await PaymentMethodDAO.Instance.UpdatePaymentMethodAsync(
                    selectedPayment.Id,
                    name,
                    description,
                    isActive,
                    imagePath);

                if (success)
                {
                    await LoadPaymentMethod();
                    await DialogHelper.ShowSuccessDialog("Thành công", "Cập nhật phương thức thành công!", this.XamlRoot);
                }
                else
                {
                    await DialogHelper.ShowErrorDialog("Lỗi", "Có lỗi khi cập nhật phương thức thanh toán. Vui lòng thử lại.", this.XamlRoot);
                }
            }
            else
            {
                await DialogHelper.ShowErrorDialog("Lỗi", "Vui lòng chọn một phương thức.", this.XamlRoot);
            }
        }

        private async void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (PaymentListView.SelectedItem is PaymentMethod selectedPayment)
            {
                var result = await DialogHelper.ShowConfirmDialog(
                    "Xác nhận",
                    $"Bạn có chắc chắn muốn xóa phương thức thanh toán '{selectedPayment.Name}' không?",
                    "Xóa",
                    "Hủy",
                    this.XamlRoot
                );

                if (result == ContentDialogResult.Primary)
                {
                    if (!await ValidatePaymentMethodExists(selectedPayment.Id))
                    {
                        await DialogHelper.ShowErrorDialog("Lỗi", "Không tìm thấy phương thức thanh toán", this.XamlRoot);
                        return;
                    }

                    bool isDeleted = await PaymentMethodDAO.Instance.DeletePaymentMethodAsync(selectedPayment.Id);

                    if (isDeleted)
                    {
                        await LoadPaymentMethod();
                        ClearFormFields();
                        await DialogHelper.ShowSuccessDialog("Thành công", "Xóa phương thức thanh toán thành công!", this.XamlRoot);
                    }
                    else
                    {
                        await DialogHelper.ShowErrorDialog("Lỗi", "Không thể xóa phương thức. Vui lòng thử lại!", this.XamlRoot);
                    }
                }
            }
            else
            {
                await DialogHelper.ShowErrorDialog("Lỗi", "Vui lòng chọn một phương thức để xóa.", this.XamlRoot);
            }
        }

        private async void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            string name = PaymentNameBox.Text.Trim();
            string description = PaymentDescriptionBox.Text.Trim();

            // Empty data validation
            if (string.IsNullOrWhiteSpace(name))
            {
                await DialogHelper.ShowErrorDialog("Lỗi", "Vui lòng nhập tên phương thức thanh toán", this.XamlRoot);
                return;
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                await DialogHelper.ShowErrorDialog("Lỗi", "Vui lòng nhập mô tả phương thức thanh toán", this.XamlRoot);
                return;
            }

            if (string.IsNullOrWhiteSpace(selectedImagePath))
            {
                await DialogHelper.ShowErrorDialog("Lỗi", "Vui lòng chọn hình ảnh cho phương thức thanh toán", this.XamlRoot);
                return;
            }

            // Data type validation
            if (!ValidationHelper.IsValidName(name))
            {
                await DialogHelper.ShowErrorDialog("Lỗi", "Tên phương thức thanh toán không hợp lệ. Chỉ được chứa chữ cái, số và khoảng trắng", this.XamlRoot);
                return;
            }

            if (!ValidationHelper.IsValidLength(description, 200))
            {
                await DialogHelper.ShowErrorDialog("Lỗi", "Mô tả không được vượt quá 200 ký tự", this.XamlRoot);
                return;
            }

            if (!await ValidationHelper.IsValidImagePath(selectedImagePath))
            {
                await DialogHelper.ShowErrorDialog("Lỗi", "Đường dẫn hình ảnh không hợp lệ", this.XamlRoot);
                return;
            }

            // Data consistency validation
            if (await ValidatePaymentNameExists(name))
            {
                await DialogHelper.ShowErrorDialog("Lỗi", "Tên phương thức thanh toán đã tồn tại", this.XamlRoot);
                return;
            }

            int isAdded = await PaymentMethodDAO.Instance.InsertPaymentMethodAsync(
                name,
                description,
                (bool)StatusComboBox.SelectedValue,
                selectedImagePath);

            if (isAdded != -1)
            {
                await DialogHelper.ShowSuccessDialog("Thành công", "Phương thức đã được thêm thành công!", this.XamlRoot);
                await LoadPaymentMethod();
                ClearFormFields();
            }
            else
            {
                await DialogHelper.ShowErrorDialog("Lỗi", "Có lỗi khi thêm phương thức vào cơ sở dữ liệu. Vui lòng thử lại.", this.XamlRoot);
            }
        }

        private void LoadStatusIntoCombobox(ComboBox cb)
        {
            cb.ItemsSource = allStatus;
            cb.DisplayMemberPath = "Value";
            cb.SelectedValuePath = "Key";   
        }

        private Task SetCategoryComboBoxAsync(bool isActive)
        {
            StatusComboBox.SelectedValue = isActive;
            return Task.CompletedTask;
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearFormFields();
            PaymentListView.SelectedItem = null;
        }

        private void ClearFormFields()
        {
            PaymentIDBox.Text = "";
            PaymentNameBox.Text = "";
            PaymentDescriptionBox.Text = "";
            selectedImagePath = string.Empty;
            PaymentImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/default-image.jpg"));
        }

        public async Task LoadPaymentMethod()
        {
            List<PaymentMethod> paymentList = await PaymentMethodDAO.Instance.GetAllPaymentMethodAsync();
            PaymentListView.ItemsSource = paymentList;
        }

        void AddPaymentBinding()
        {
            var paymentIDBinding = new Binding
            {
                Path = new PropertyPath("Id"),
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };
            PaymentIDBox.SetBinding(TextBox.TextProperty, paymentIDBinding);

            var paymentNameBinding = new Binding
            {
                Path = new PropertyPath("Name"),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };
            PaymentNameBox.SetBinding(TextBox.TextProperty, paymentNameBinding);

            var paymentDescriptionBinding = new Binding
            {
                Path = new PropertyPath("Description"),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };
            PaymentDescriptionBox.SetBinding(TextBox.TextProperty, paymentDescriptionBinding);
        }

        private void PaymentListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedPayment = (PaymentMethod)PaymentListView.SelectedItem;

            if (selectedPayment != null)
            {
                this.DataContext = selectedPayment;
                PaymentIDBox.Text = selectedPayment.Id.ToString();
                Debug.WriteLine(selectedPayment.IconUrl);
                _ = SetCategoryComboBoxAsync(selectedPayment.IsActive);

                // Cập nhật hình ảnh
                if (!string.IsNullOrEmpty(selectedPayment.IconUrl) || selectedPayment.IconUrl!.Equals(""))
                {
                    try
                    {
                        PaymentImage.Source = new BitmapImage(new Uri("ms-appx://" + selectedPayment.IconUrl));
                    }
                    catch
                    {
                        PaymentImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/default-image.jpg"));
                    }
                }
                else
                {
                    PaymentImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/default-image.jpg"));
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
                    StorageFolder paymentMethodFolder = await assetsFolder.GetFolderAsync("Payment");

                    StorageFile copiedFile = await file.CopyAsync(paymentMethodFolder, file.Name, NameCollisionOption.GenerateUniqueName);

                    selectedImagePath = $"/Assets/Payment/{copiedFile.Name}";

                    BitmapImage bitmapImage = new BitmapImage();
                    using (var stream = await file.OpenReadAsync())
                    {
                        await bitmapImage.SetSourceAsync(stream);
                    }
                    PaymentImage.Source = bitmapImage;
                }
                catch (Exception ex)
                {
                    await DialogHelper.ShowErrorDialog("Lỗi", $"Không thể sao chép tệp hình ảnh: {ex.Message}", this.XamlRoot);
                }
            }
        }
    }
}
