using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CafePOS.DAO;
using CafePOS.DTO;

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
                await ShowMessageAsync("Lỗi tải khách hàng: " + ex.Message);
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
                var confirm = new ContentDialog
                {
                    Title = "Confirm Deletion",
                    Content = $"Bạn có chắc chắn xóa '{guest.Name}'?",
                    PrimaryButtonText = "Xóa",
                    CloseButtonText = "Hủy",
                    XamlRoot = this.XamlRoot
                };

                var result = await confirm.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    bool success = await GuestDAO.Instance.DeleteGuestAsync(guest.Id);
                    if (success) await LoadGuestsAsync();
                    else await ShowMessageAsync("Xóa khách hàng thất bại.");
                }
            }
        }

        private async void SaveGuestDialog_Click(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            string name = NameTextBox.Text.Trim();
            string phone = PhoneTextBox.Text.Trim();
            string email = EmailTextBox.Text.Trim();
            string notes = NotesTextBox.Text.Trim();
            int.TryParse(PointsTextBox.Text, out int totalPoints);
            DateTime memberSince = MemberSinceDatePicker.Date.DateTime;

            string membership = Guest.GetMembershipLevel(totalPoints);

            int availablePoints = totalPoints; // ✅ Gán availablePoints bằng totalPoints ban đầu

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(phone))
            {
                args.Cancel = true;
                await ShowMessageAsync("Hãy nhập tên và số điện thoại.");
                return;
            }

            if (selectedGuest == null)
            {
                await GuestDAO.Instance.AddGuestAsync(
                    name, phone, email, notes, totalPoints, availablePoints, membership, DateOnly.FromDateTime(memberSince)
                );
            }
            else
            {
                await GuestDAO.Instance.UpdateGuestAsync(
                    selectedGuest.Id, name, phone, email, notes, totalPoints, availablePoints, membership, DateOnly.FromDateTime(memberSince)
                );
            }

            await LoadGuestsAsync();
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
