using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;
using CafePOS.DAO;
using CafePOS.DTO;
using System.Collections.Generic;
using System.Linq;
using CafePOS.Utilities;
using System.Text.RegularExpressions;

namespace CafePOS
{
    public sealed partial class StaffPage : Page
    {
        private Staff? selectedStaff;
        private List<Staff> allStaff = new List<Staff>();

        public StaffPage()
        {
            this.InitializeComponent();
            _ = InitializePageAsync();
        }

        private async Task InitializePageAsync()
        {
            await LoadStaffAsync();
        }

        private async Task LoadStaffAsync()
        {
            try
            {
                allStaff = await StaffDAO.Instance.GetAllStaffAsync();
                StaffListView.ItemsSource = allStaff;
            }
            catch (Exception ex)
            {
                await DialogHelper.ShowErrorDialog("Lỗi", "Lỗi tải dữ liệu: " + ex.Message, this.XamlRoot);
            }
        }

        private void AddStaffButton_Click(object sender, RoutedEventArgs e)
        {
            selectedStaff = null;
            ClearForm();

            StaffDialog.Title = "Thêm nhân viên";

            UsernameTextBox.Visibility = Visibility.Visible;
            PasswordBox.Visibility = Visibility.Visible;
            UsernameTextReadonlyBox.Visibility = Visibility.Collapsed;
            PasswordTextReadonlyBox.Visibility = Visibility.Collapsed;


            _ = StaffDialog.ShowAsync();
        }


        private async void EditStaff_Click(object sender, RoutedEventArgs e)
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

                UsernameTextBox.Visibility = Visibility.Collapsed;
                PasswordBox.Visibility = Visibility.Collapsed;
                UsernameTextReadonlyBox.Visibility = Visibility.Visible;
                PasswordTextReadonlyBox.Visibility = Visibility.Visible;

                UsernameTextReadonlyBox.Text = staff.UserName ?? "";

                try
                {
                    var account = await AccountDAO.Instance.GetAccountByUserNameAsync(staff.UserName ?? "");
                    PasswordTextReadonlyBox.Text = account?.Password ?? "(không có)";
                }
                catch (Exception ex)
                {
                    PasswordTextReadonlyBox.Text = "(lỗi tải mật khẩu)";
                }

                StaffDialog.Title = "Chỉnh sửa nhân viên";
                _ = StaffDialog.ShowAsync();
            }
        }



        private async void DeleteStaff_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is Staff staff)
            {
                var result = await DialogHelper.ShowConfirmDialog(
                    "Xác nhận xoá", 
                    $"Bạn có chắc muốn xoá nhân viên '{staff.Name}' không?", 
                    "Xoá", 
                    "Huỷ", 
                    this.XamlRoot);

                if (result == ContentDialogResult.Primary)
                {
                    bool success = await StaffDAO.Instance.DeleteStaffAsync(staff.Id);
                    if (success)
                    {
                        if (!string.IsNullOrEmpty(staff.UserName))
                        {
                            await AccountDAO.Instance.DeleteAccountByUserNameAsync(staff.UserName);
                        }

                        await LoadStaffAsync();
                    }
                    else
                    {
                        await DialogHelper.ShowMessageAsync("Không thể xoá nhân viên.", this.XamlRoot);
                    }
                }
            }
        }


        private async Task<bool> ValidateInputs(ContentDialogButtonClickEventArgs args)
        {
            // Clear any previous validation messages
            ValidationInfoBar.IsOpen = false;

            string name = NameTextBox.Text?.Trim() ?? "";
            string phone = PhoneTextBox.Text?.Trim() ?? "";
            string email = EmailTextBox.Text?.Trim() ?? "";
            string position = PositionTextBox.Text?.Trim() ?? "";
            string salaryText = SalaryTextBox.Text?.Trim() ?? "";
            string username = UsernameTextBox.Text?.Trim() ?? "";
            string password = PasswordBox.Password?.Trim() ?? "";
            DateTimeOffset? dob = DobDatePicker.Date;

            bool isCreateMode = selectedStaff == null;

            // Validate empty data
            if (string.IsNullOrWhiteSpace(name))
            {
                ShowValidationError("Tên nhân viên không được để trống!");
                args.Cancel = true;
                return false;
            }

            if (dob == null)
            {
                ShowValidationError("Vui lòng chọn ngày sinh!");
                args.Cancel = true;
                return false;
            }
            else
            {
                if (dob.Value > DateTime.Today)
                {
                    ShowValidationError("Ngày sinh không được lớn hơn ngày hiện tại!");
                    args.Cancel = true;
                    return false;
                }

                DateTime sixteenYearsAgo = DateTime.Today.AddYears(-16);
                if (dob.Value > sixteenYearsAgo)
                {
                    ShowValidationError("Nhân viên phải từ 16 tuổi trở lên!");
                    args.Cancel = true;
                    return false;
                }
            }

            if (string.IsNullOrWhiteSpace(phone))
            {
                ShowValidationError("Số điện thoại không được để trống!");
                args.Cancel = true;
                return false;
            }
            else
            {
                if (!Regex.IsMatch(phone, @"^0\d{9,10}$"))
                {
                    ShowValidationError("Số điện thoại không đúng định dạng Việt Nam (bắt đầu bằng số 0 và có 10 hoặc 11 số)!");
                    args.Cancel = true;
                    return false;
                }
            }

            if (!string.IsNullOrWhiteSpace(email) && !Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                ShowValidationError("Email không đúng định dạng!");
                args.Cancel = true;
                return false;
            }

            if (string.IsNullOrWhiteSpace(position))
            {
                ShowValidationError("Chức vụ không được để trống!");
                args.Cancel = true;
                return false;
            }

            if (string.IsNullOrWhiteSpace(salaryText))
            {
                ShowValidationError("Lương không được để trống!");
                args.Cancel = true;
                return false;
            }

            if (isCreateMode)
            {
                if (string.IsNullOrWhiteSpace(username))
                {
                    ShowValidationError("Tên đăng nhập không được để trống!");
                    args.Cancel = true;
                    return false;
                }

                if (string.IsNullOrWhiteSpace(password))
                {
                    ShowValidationError("Mật khẩu không được để trống!");
                    args.Cancel = true;
                    return false;
                }
            }

            // Validate data type and format
            if (!float.TryParse(salaryText, out float salary) || salary <= 0)
            {
                ShowValidationError("Lương phải là số dương!");
                args.Cancel = true;
                return false;
            }

            if (!string.IsNullOrWhiteSpace(email) && !ValidationHelper.IsValidEmail(email))
            {
                ShowValidationError("Email không đúng định dạng!");
                args.Cancel = true;
                return false;
            }

            if (!ValidationHelper.IsValidPhoneNumber(phone))
            {
                ShowValidationError("Số điện thoại không đúng định dạng!");
                args.Cancel = true;
                return false;
            }

            // Validate value range
            if (dob.Value > DateTime.Today)
            {
                ShowValidationError("Ngày sinh không được lớn hơn ngày hiện tại!");
                args.Cancel = true;
                return false;
            }

            // Validate data consistency
            if (isCreateMode)
            {
                if (allStaff.Any(s => s.UserName?.Equals(username, StringComparison.OrdinalIgnoreCase) == true))
                {
                    ShowValidationError("Tên đăng nhập đã tồn tại!");
                    args.Cancel = true;
                    return false;
                }
            }

            return true;
        }

        // Helper method to show validation errors in the InfoBar
        private void ShowValidationError(string errorMessage)
        {
            ValidationInfoBar.Title = "Lỗi xác thực";
            ValidationInfoBar.Message = errorMessage;
            ValidationInfoBar.Severity = InfoBarSeverity.Error;
            ValidationInfoBar.IsOpen = true;
        }

        private async void SaveStaffDialog_Click(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (!await ValidateInputs(args))
                return;

            string name = NameTextBox.Text.Trim();
            string dob = DobDatePicker.Date.ToString("yyyy-MM-dd");
            string gender = (GenderComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Nam";
            string phone = PhoneTextBox.Text.Trim();
            string email = EmailTextBox.Text.Trim();
            string position = PositionTextBox.Text.Trim();
            float.TryParse(SalaryTextBox.Text, out float salary);
            string username = UsernameTextBox.Text.Trim();
            string password = PasswordBox.Password.Trim();

            if (selectedStaff == null)
            {
                string displayName = name;
                bool accountCreated = await AccountDAO.Instance.CreateAccountAsync(username, displayName, password, 0);

                if (!accountCreated)
                {
                    args.Cancel = true;
                    await DialogHelper.ShowMessageAsync("Tạo tài khoản đăng nhập thất bại.", this.XamlRoot);
                    return;
                }

                bool success = await StaffDAO.Instance.AddStaffAsync(name, dob, gender, phone, email, position, salary, username);

                if (success)
                {
                    await LoadStaffAsync();
                }
                else
                {
                    args.Cancel = true;
                    await AccountDAO.Instance.DeleteAccountByUserNameAsync(username);
                    await DialogHelper.ShowMessageAsync("Lưu thông tin thất bại. Tài khoản đã được rollback.", this.XamlRoot);
                }
            }
            else
            {
                bool success = await StaffDAO.Instance.UpdateStaffAsync(selectedStaff.Id, name, dob, gender, phone, email, position, salary);
                if (success)
                {
                    await LoadStaffAsync();
                }
                else
                {
                    args.Cancel = true;
                    await DialogHelper.ShowMessageAsync("Cập nhật thông tin thất bại.", this.XamlRoot);
                }
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

            UsernameTextReadonlyBox.Text = "";
            PasswordTextReadonlyBox.Text = "";
            UsernameTextReadonlyBox.Visibility = Visibility.Collapsed;
            PasswordTextReadonlyBox.Visibility = Visibility.Collapsed;
        }
    }
}
