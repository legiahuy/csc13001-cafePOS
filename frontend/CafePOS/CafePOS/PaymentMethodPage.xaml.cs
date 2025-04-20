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

        private async void btnView_Click(object sender, RoutedEventArgs e)
        {
            await LoadPaymentMethod();
        }

        private async void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (PaymentListView.SelectedItem is PaymentMethod selectedPayment)
            {
                string name = PaymentNameBox.Text;
                string imagePath = string.IsNullOrEmpty(selectedImagePath) ? selectedPayment.IconUrl! : selectedImagePath;
                string description = PaymentDescriptionBox.Text;
                bool isActive = (bool)StatusComboBox.SelectedValue;

                bool success = await PaymentMethodDAO.Instance.UpdatePaymentMethodAsync(
                    selectedPayment.Id,
                    name,
                    description,
                    isActive,
                    imagePath);

                if (success)
                {
                    await LoadPaymentMethod();
                    var successDialog = new ContentDialog
                    {
                        Title = "Thành công",
                        Content = "Cập nhật phương thức thành công!",
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
                        Content = "Có lỗi khi cập nhật phương thức thanh toán. Vui lòng thử lại.",
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
                    Content = "Vui lòng chọn một phương thức.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await errorDialog.ShowAsync();
            }
        }

        private async void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (PaymentListView.SelectedItem is PaymentMethod selectedPayment)
            {
                ContentDialog confirmDialog = new ContentDialog
                {
                    Title = "Xác nhận",
                    Content = $"Bạn có chắc chắn muốn xóa phương thức thanh toán '{selectedPayment.Name}' không?",
                    PrimaryButtonText = "Xóa",
                    CloseButtonText = "Hủy",
                    XamlRoot = this.XamlRoot
                };

                ContentDialogResult result = await confirmDialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    bool isDeleted = await PaymentMethodDAO.Instance.DeletePaymentMethodAsync(selectedPayment.Id);
                    Debug.WriteLine(selectedPayment.Id);

                    if (isDeleted)
                    {
                        await LoadPaymentMethod();
                        ClearFormFields();
                    }
                    else
                    {
                        ContentDialog dialog = new ContentDialog
                        {
                            Title = "Lỗi",
                            Content = "Không thể xóa phương thức. Vui lòng thử lại!",
                            CloseButtonText = "OK",
                            XamlRoot = this.XamlRoot
                        };
                        await dialog.ShowAsync();
                    }
                }
            }
            else
            {
                var errorDialog = new ContentDialog
                {
                    Title = "Lỗi",
                    Content = "Vui lòng chọn một phương thức để xóa.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await errorDialog.ShowAsync();
            }
        }

        private async void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            string name = PaymentNameBox.Text;
            string description = PaymentDescriptionBox.Text;

            if (string.IsNullOrWhiteSpace(name) || name.Equals(""))
            {
                var dialog = new ContentDialog
                {
                    Title = "Lỗi",
                    Content = "Vui lòng điền đầy đủ thông tin cho phương thức!",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await dialog.ShowAsync();
                return;
            }

            int isAdded = await PaymentMethodDAO.Instance.InsertPaymentMethodAsync(
                name,
                description,
                (bool)StatusComboBox.SelectedValue,
                selectedImagePath);

            if (isAdded != -1)
            {
                var successDialog = new ContentDialog
                {
                    Title = "Thành công",
                    Content = "Phương thức đã được thêm thành công!",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await successDialog.ShowAsync();

                await LoadPaymentMethod();
                ClearFormFields();
            }
            else
            {
                var errorDialog = new ContentDialog
                {
                    Title = "Lỗi",
                    Content = "Có lỗi khi thêm phương thức vào cơ sở dữ liệu. Vui lòng thử lại.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await errorDialog.ShowAsync();
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
    }
}
