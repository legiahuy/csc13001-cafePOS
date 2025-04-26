using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CafePOS.DAO;
using CafePOS.DTO;
using System.Linq;
using CafePOS.Utilities;

namespace CafePOS
{
    public sealed partial class CustomersPage : Page
    {
        private Guest? selectedGuest;

        public CustomersPage()
        {
            this.InitializeComponent();
            LoadGuestsAsync();
        }

        private async Task LoadGuestsAsync()
        {
            try
            {
                List<Guest> guestList = await GuestDAO.Instance.GetAllGuestsAsync();

                // Cập nhật MembershipLevel dựa vào TotalPoints
                foreach (var guest in guestList)
                {
                    guest.MembershipLevel = Guest.GetMembershipLevel(guest.TotalPoints);
                }

                GuestListView.ItemsSource = guestList;
            }
            catch (Exception ex)
            {
                await DialogHelper.ShowMessageAsync("Lỗi tải khách hàng: " + ex.Message, this.XamlRoot);
            }
        }

        private void AddGuestButton_Click(object sender, RoutedEventArgs e)
        {
            selectedGuest = null;
            ClearForm();
            GuestDialog.Title = "Thêm khách hàngs";
            _ = GuestDialog.ShowAsync();
        }

        private void EditGuest_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is Guest guest)
            {
                selectedGuest = guest;

                NameTextBox.Text = guest.Name ?? "";
                PhoneTextBox.Text = guest.Phone ?? "";
                EmailTextBox.Text = guest.Email ?? "";
                PointsTextBox.Text = guest.TotalPoints.ToString();
                MemberSinceDatePicker.Date = new DateTimeOffset(guest.MemberSince);

                foreach (ComboBoxItem item in MembershipComboBox.Items)
                {
                    if (item.Content?.ToString() == Guest.GetMembershipLevel(guest.TotalPoints))
                    {
                        MembershipComboBox.SelectedItem = item;
                        break;
                    }
                }

                NotesTextBox.Text = guest.Notes ?? "";
                GuestDialog.Title = "Chỉnh sửa thông tin khách hàng";
                _ = GuestDialog.ShowAsync();
            }
        }

        private async void DeleteGuest_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is Guest guest)
            {
                var result = await DialogHelper.ShowConfirmDialog(
                    "Confirm Deletion", 
                    $"Bạn có chắc chắn xóa '{guest.Name}'?", 
                    "Xóa", 
                    "Hủy", 
                    this.XamlRoot
                );

                if (result == ContentDialogResult.Primary)
                {
                    bool success = await GuestDAO.Instance.DeleteGuestAsync(guest.Id);
                    if (success) await LoadGuestsAsync();
                    else await DialogHelper.ShowMessageAsync("Xóa khách hàng thất bại.", this.XamlRoot);
                }
            }
        }

        private async Task<bool> ValidatePhoneNumberExists(string phone, int? excludeId = null)
        {
            var guests = await GuestDAO.Instance.GetAllGuestsAsync();
            return guests.Any(g => g.Phone == phone && g.Id != excludeId);
        }

        private async Task<bool> ValidateEmailExists(string email, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;
                
            var guests = await GuestDAO.Instance.GetAllGuestsAsync();
            return guests.Any(g => g.Email == email && g.Id != excludeId);
        }

        private async Task<bool> ValidateGuestExists(int id)
        {
            var guests = await GuestDAO.Instance.GetAllGuestsAsync();
            return guests.Any(g => g.Id == id);
        }

        private async void SaveGuestDialog_Click(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            string name = NameTextBox.Text.Trim();
            string phone = PhoneTextBox.Text.Trim();
            string email = EmailTextBox.Text.Trim();
            string notes = NotesTextBox.Text.Trim();
            int.TryParse(PointsTextBox.Text, out int totalPoints);
            DateTime memberSince = MemberSinceDatePicker.Date.DateTime;

            // Empty data validation
            if (string.IsNullOrWhiteSpace(name))
            {
                args.Cancel = true;
                await DialogHelper.ShowErrorDialog("Lỗi", "Vui lòng nhập tên khách hàng", this.XamlRoot);
                return;
            }

            if (string.IsNullOrWhiteSpace(phone))
            {
                args.Cancel = true;
                await DialogHelper.ShowErrorDialog("Lỗi", "Vui lòng nhập số điện thoại", this.XamlRoot);
                return;
            }

            // Data type validation
            if (!ValidationHelper.IsValidPhoneNumber(phone))
            {
                args.Cancel = true;
                await DialogHelper.ShowErrorDialog("Lỗi", "Số điện thoại không hợp lệ. Vui lòng nhập số điện thoại 10-11 chữ số, bắt đầu bằng số 0", this.XamlRoot);
                return;
            }

            if (!ValidationHelper.IsValidEmail(email))
            {
                args.Cancel = true;
                await DialogHelper.ShowErrorDialog("Lỗi", "Email không hợp lệ", this.XamlRoot);
                return;
            }

            if (!ValidationHelper.IsValidPositiveNumber(PointsTextBox.Text))
            {
                args.Cancel = true;
                await DialogHelper.ShowErrorDialog("Lỗi", "Điểm tích lũy phải là số nguyên không âm", this.XamlRoot);
                return;
            }

            if (!ValidationHelper.IsValidPastDate(memberSince))
            {
                args.Cancel = true;
                await DialogHelper.ShowErrorDialog("Lỗi", "Ngày tham gia không được trong tương lai", this.XamlRoot);
                return;
            }

            // Data consistency validation
            if (selectedGuest == null)
            {
                if (await ValidatePhoneNumberExists(phone))
                {
                    args.Cancel = true;
                    await DialogHelper.ShowErrorDialog("Lỗi", "Số điện thoại đã tồn tại", this.XamlRoot);
                    return;
                }

                if (!string.IsNullOrWhiteSpace(email) && await ValidateEmailExists(email))
                {
                    args.Cancel = true;
                    await DialogHelper.ShowErrorDialog("Lỗi", "Email đã tồn tại", this.XamlRoot);
                    return;
                }
            }
            else
            {
                if (!await ValidateGuestExists(selectedGuest.Id))
                {
                    args.Cancel = true;
                    await DialogHelper.ShowErrorDialog("Lỗi", "Không tìm thấy thông tin khách hàng", this.XamlRoot);
                    return;
                }

                if (await ValidatePhoneNumberExists(phone, selectedGuest.Id))
                {
                    args.Cancel = true;
                    await DialogHelper.ShowErrorDialog("Lỗi", "Số điện thoại đã tồn tại", this.XamlRoot);
                    return;
                }

                if (!string.IsNullOrWhiteSpace(email) && await ValidateEmailExists(email, selectedGuest.Id))
                {
                    args.Cancel = true;
                    await DialogHelper.ShowErrorDialog("Lỗi", "Email đã tồn tại", this.XamlRoot);
                    return;
                }
            }

            string membership = Guest.GetMembershipLevel(totalPoints);
            int availablePoints = totalPoints;

            try
            {
                if (selectedGuest == null)
                {
                    await GuestDAO.Instance.AddGuestAsync(
                        name, phone, email, notes, totalPoints, availablePoints, membership, DateOnly.FromDateTime(memberSince)
                    );
                    await DialogHelper.ShowSuccessDialog("Thành công", "Đã thêm khách hàng thành công", this.XamlRoot);
                }
                else
                {
                    await GuestDAO.Instance.UpdateGuestAsync(
                        selectedGuest.Id, name, phone, email, notes, totalPoints, availablePoints, membership, DateOnly.FromDateTime(memberSince)
                    );
                    await DialogHelper.ShowSuccessDialog("Thành công", "Đã cập nhật thông tin khách hàng thành công", this.XamlRoot);
                }

                await LoadGuestsAsync();
            }
            catch (Exception ex)
            {
                args.Cancel = true;
                await DialogHelper.ShowErrorDialog("Lỗi", $"Có lỗi xảy ra: {ex.Message}", this.XamlRoot);
            }
        }

        private void CancelGuestDialog_Click(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            NameTextBox.Text = "";
            PhoneTextBox.Text = "";
            EmailTextBox.Text = "";
            PointsTextBox.Text = "0";
            NotesTextBox.Text = "";
            MemberSinceDatePicker.Date = new DateTimeOffset(DateTime.Today);
            MembershipComboBox.SelectedIndex = 0;
        }
    }
}
