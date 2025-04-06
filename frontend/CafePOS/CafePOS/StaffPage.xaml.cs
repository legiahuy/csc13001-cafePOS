using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;
using CafePOS.DAO;
using CafePOS.DTO;

namespace CafePOS
{
    public sealed partial class StaffPage : Page
    {
        private Staff? selectedStaff; 

        public StaffPage()
        {
            this.InitializeComponent();
            LoadStaffAsync();
        }

        private async Task LoadStaffAsync()
        {
            try
            {
                var staffList = await StaffDAO.Instance.GetAllStaffAsync();
                StaffListView.ItemsSource = staffList;
            }
            catch (Exception ex)
            {
                await ShowMessageAsync("Lỗi tải dữ liệu: " + ex.Message);
            }
        }

        private void AddStaffButton_Click(object sender, RoutedEventArgs e)
        {
            selectedStaff = null;
            ClearForm();
            StaffDialog.Title = "Thêm nhân viên";
            _ = StaffDialog.ShowAsync();
        }

        private void EditStaff_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is Staff staff)
            {
                selectedStaff = staff;
                NameTextBox.Text = staff.Name ?? "";
                DobDatePicker.Date = DateTime.TryParse(staff.Dob, out var date) ? date : DateTime.Today;
                GenderComboBox.SelectedItem = staff.Gender ?? "Nam";
                PhoneTextBox.Text = staff.Phone ?? "";
                EmailTextBox.Text = staff.Email ?? "";
                PositionTextBox.Text = staff.Position ?? "";
                SalaryTextBox.Text = staff.Salary.ToString();

                StaffDialog.Title = "Chỉnh sửa nhân viên";
                _ = StaffDialog.ShowAsync();
            }
        }

        private async void DeleteStaff_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is Staff staff)
            {
                var confirm = new ContentDialog
                {
                    Title = "Xác nhận xoá",
                    Content = $"Bạn có chắc muốn xoá nhân viên '{staff.Name}' không?",
                    PrimaryButtonText = "Xoá",
                    CloseButtonText = "Huỷ",
                    XamlRoot = this.XamlRoot
                };

                var result = await confirm.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    bool success = await StaffDAO.Instance.DeleteStaffAsync(staff.Id);
                    if (success)
                    {
                        await LoadStaffAsync();
                    }
                    else
                    {
                        await ShowMessageAsync("Không thể xoá nhân viên.");
                    }
                }
            }
        }

        private async void SaveStaffDialog_Click(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            string name = NameTextBox.Text.Trim();
            string dob = DobDatePicker.Date.ToString("yyyy-MM-dd");
            string gender = (GenderComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Nam";
            string phone = PhoneTextBox.Text.Trim();
            string email = EmailTextBox.Text.Trim();
            string position = PositionTextBox.Text.Trim();
            float.TryParse(SalaryTextBox.Text, out float salary);

            if (string.IsNullOrWhiteSpace(name))
            {
                args.Cancel = true;
                await ShowMessageAsync("Vui lòng nhập tên nhân viên.");
                return;
            }

            bool success;
            if (selectedStaff == null)
            {
                success = await StaffDAO.Instance.AddStaffAsync(name, dob, gender, phone, email, position, salary);
            }
            else
            {
                success = await StaffDAO.Instance.UpdateStaffAsync(selectedStaff.Id, name, dob, gender, phone, email, position, salary);
            }

            if (success)
            {
                await LoadStaffAsync();
            }
            else
            {
                args.Cancel = true;
                await ShowMessageAsync("Lưu thông tin thất bại.");
            }
        }

        private void CancelStaffDialog_Click(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            NameTextBox.Text = "";
            DobDatePicker.Date = DateTime.Today;
            GenderComboBox.SelectedIndex = 0;
            PhoneTextBox.Text = "";
            EmailTextBox.Text = "";
            PositionTextBox.Text = "";
            SalaryTextBox.Text = "";
        }

        private async Task ShowMessageAsync(string message)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = "Thông báo",
                Content = message,
                CloseButtonText = "Đóng",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
        }
    }
}
