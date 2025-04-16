using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CafePOS.DAO;
using CafePOS.DTO;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Windows.Foundation;
using Windows.Graphics;
using WinRT.Interop;

namespace CafePOS
{
    public sealed partial class CheckoutWindow : Window
    {
        private CafeTable _currentTable;
        private int _currentBillId;
        private List<Menu> _orderItems;
        private double _originalTotal = 0;
        private Guest _selectedGuest;
        private List<Guest> _guestList;
        private List<PaymentMethod> _paymentMethods;
        private CultureInfo _culture = new CultureInfo("vi-VN");
        private AppWindow _appWindow;

        public event EventHandler<bool> CheckoutCompleted;

        public CheckoutWindow()
        {
            this.InitializeComponent();

            // Get the root element of your window content and attach the Loaded event
            if (this.Content is FrameworkElement rootElement)
            {
                rootElement.Loaded += RootElement_Loaded;
            }

            // Lấy AppWindow từ Window hiện tại
            IntPtr windowHandle = WindowNative.GetWindowHandle(this);
            WindowId windowId = Win32Interop.GetWindowIdFromWindow(windowHandle);
            _appWindow = AppWindow.GetFromWindowId(windowId);

            // Thiết lập kích thước mặc định
            _appWindow.Resize(new SizeInt32(800, 600));

            this.Title = "Thanh toán";

            LoadData();
        }

        private void RootElement_Loaded(object sender, RoutedEventArgs e)
        {
            // Now it's safe to update UI elements
            if (_currentTable != null)
            {
                TableNameTextBlock.Text = $"Bàn: {_currentTable.Name}";
                OrderSummaryListView.ItemsSource = _orderItems;
                UpdatePrices(0);
            }
        }

        public void Initialize(CafeTable table, int billId, List<Menu> orderItems, double totalAmount)
        {
            _currentTable = table;
            _currentBillId = billId;
            _orderItems = orderItems;
            _originalTotal = totalAmount;

            // Thiết lập tiêu đề cửa sổ
            this.Title = $"Thanh toán bàn {table.Name}";

            // Căn giữa cửa sổ
            CenterOnScreen();

            // Cập nhật UI
            if (this.Content != null)
            {
                TableNameTextBlock.Text = $"Bàn: {table.Name}";
                OrderSummaryListView.ItemsSource = orderItems;
                UpdatePrices(0);
            }
        }

        // Phương thức căn giữa cửa sổ
        private void CenterOnScreen()
        {
            DisplayArea displayArea = DisplayArea.GetFromWindowId(Win32Interop.GetWindowIdFromWindow(
                WindowNative.GetWindowHandle(this)), DisplayAreaFallback.Primary);

            if (displayArea != null)
            {
                // Lấy các thông số của màn hình
                var workAreaWidth = displayArea.WorkArea.Width;
                var workAreaHeight = displayArea.WorkArea.Height;

                // Lấy kích thước hiện tại của cửa sổ
                var windowWidth = _appWindow.Size.Width;
                var windowHeight = _appWindow.Size.Height;

                // Tính toán vị trí để căn giữa
                var newX = (workAreaWidth - windowWidth) / 2;
                var newY = (workAreaHeight - windowHeight) / 2;

                // Di chuyển cửa sổ đến vị trí mới
                _appWindow.Move(new PointInt32(newX, newY));
            }
        }

        private void LoadData()
        {
            LoadPaymentMethods();
            LoadGuests();
        }

        private async void LoadPaymentMethods()
        {
            try
            {
                _paymentMethods = await PaymentMethodDAO.Instance.GetAllActivePaymentMethodsAsync();
                PaymentMethodComboBox.ItemsSource = _paymentMethods;
                if (_paymentMethods.Count > 0)
                {
                    PaymentMethodComboBox.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                await ShowDialog("Lỗi", $"Không thể tải phương thức thanh toán: {ex.Message}");
            }
        }

        private async void LoadGuests()
        {
            try
            {
                _guestList = await GuestDAO.Instance.GetAllGuestsAsync();
            }
            catch (Exception ex)
            {
                await ShowDialog("Lỗi", $"Không thể tải danh sách khách hàng: {ex.Message}");
            }
        }

        private void UpdatePrices(double discountPercent)
        {
            if (TotalPriceTextBlock != null)
            {
                TotalPriceTextBlock.Text = _originalTotal.ToString("C0", _culture);
            }

            double finalAmount = _originalTotal * (100 - discountPercent) / 100;

            if (FinalPriceTextBlock != null)
            {
                FinalPriceTextBlock.Text = finalAmount.ToString("C0", _culture);
            }

            if (_selectedGuest != null && PointsEarnedTextBlock != null)
            {
                // Calculate points earned (example: 1 point per 10,000 VND)
                int pointsEarned = (int)(finalAmount / 10000);
                PointsEarnedTextBlock.Text = $"Điểm tích lũy từ hóa đơn này: {pointsEarned}";
            }
        }

        private void DiscountBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            UpdatePrices((double)sender.Value);
        }

        private void GuestSearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                string query = sender.Text.ToLower();
                var suggestions = _guestList
                    .Where(g => g.Name.ToLower().Contains(query) ||
                           (g.Phone != null && g.Phone.Contains(query)))
                    .ToList();

                sender.ItemsSource = suggestions;
            }
        }

        private void GuestSearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion != null && args.ChosenSuggestion is Guest guest)
            {
                SelectGuest(guest);
            }
        }

        private void GuestSearchBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            if (args.SelectedItem is Guest guest)
            {
                SelectGuest(guest);
            }
        }

        private void SelectGuest(Guest guest)
        {
            _selectedGuest = guest;
            GuestNameTextBlock.Text = guest.Name;
            GuestPhoneTextBlock.Text = guest.Phone ?? "Không có SĐT";
            GuestPointsTextBlock.Text = $"Điểm: {guest.Points}";
            GuestLevelTextBlock.Text = $"Hạng: {guest.MembershipLevel}";

            // Calculate points earned (example: 1 point per 10,000 VND)
            double finalAmount = _originalTotal * (100 - (double)DiscountBox.Value) / 100;
            int pointsEarned = (int)(finalAmount / 10000);
            PointsEarnedTextBlock.Text = $"Điểm tích lũy từ hóa đơn này: {pointsEarned}";

            SelectedGuestPanel.Visibility = Visibility.Visible;
            GuestSearchBox.Text = guest.Name;
        }

        private void ClearGuestButton_Click(object sender, RoutedEventArgs e)
        {
            _selectedGuest = null;
            SelectedGuestPanel.Visibility = Visibility.Collapsed;
            GuestSearchBox.Text = string.Empty;
        }

        private async void AddNewGuestButton_Click(object sender, RoutedEventArgs e)
        {
            // Tạo ContentDialog
            ContentDialog dialog = new ContentDialog
            {
                Title = "Thêm khách hàng mới",
                PrimaryButtonText = "Thêm",
                CloseButtonText = "Hủy"
            };

            // Cài đặt XamlRoot từ rootElement
            FrameworkElement rootElement = this.Content as FrameworkElement;
            if (rootElement != null)
            {
                dialog.XamlRoot = rootElement.XamlRoot;
            }

            // Create guest input form
            StackPanel panel = new StackPanel { Spacing = 10 };

            TextBox nameBox = new TextBox { Header = "Tên khách hàng", PlaceholderText = "Nhập tên" };
            panel.Children.Add(nameBox);

            TextBox phoneBox = new TextBox
            {
                Header = "Số điện thoại",
                PlaceholderText = "Nhập số điện thoại",
                InputScope = new InputScope
                {
                    Names = { new InputScopeName { NameValue = InputScopeNameValue.TelephoneNumber } }
                }
            };
            panel.Children.Add(phoneBox);

            TextBox emailBox = new TextBox
            {
                Header = "Email",
                PlaceholderText = "Nhập email (tùy chọn)",
                InputScope = new InputScope
                {
                    Names = { new InputScopeName { NameValue = InputScopeNameValue.EmailNameOrAddress } }
                }
            };
            panel.Children.Add(emailBox);

            TextBox notesBox = new TextBox
            {
                Header = "Ghi chú",
                PlaceholderText = "Ghi chú (tùy chọn)",
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true,
                Height = 80
            };
            panel.Children.Add(notesBox);

            dialog.Content = panel;

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                string name = nameBox.Text?.Trim();
                string phone = phoneBox.Text?.Trim();
                string email = emailBox.Text?.Trim();
                string notes = notesBox.Text?.Trim();

                if (string.IsNullOrEmpty(name))
                {
                    await ShowDialog("Lỗi", "Vui lòng nhập tên khách hàng.");
                    return;
                }

                try
                {
                    Guest newGuest = new Guest
                    {
                        Name = name,
                        Phone = phone,
                        Email = email,
                        Notes = notes,
                        Points = 0,
                        MembershipLevel = "Regular",
                        MemberSince = DateTime.Now
                    };

                    int guestId = await GuestDAO.Instance.AddGuestAsync(name, phone, email, notes, 0, "Regular", DateOnly.FromDateTime(DateTime.Now));
                    
                    if (guestId > 0)
                    {
                        newGuest.Id = guestId;
                        _guestList.Add(newGuest);
                        SelectGuest(newGuest);
                        await ShowDialog("Thành công", "Đã thêm khách hàng mới.");
                    }
                }
                catch (Exception ex)
                {
                    await ShowDialog("Lỗi", $"Không thể thêm khách hàng: {ex.Message}");
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            CheckoutCompleted?.Invoke(this, false);
            this.Close();
        }

        private async void ConfirmPaymentButton_Click(object sender, RoutedEventArgs e)
        {
            if (PaymentMethodComboBox.SelectedItem == null)
            {
                await ShowDialog("Thông báo", "Vui lòng chọn phương thức thanh toán.");
                return;
            }

            var selectedPaymentMethod = PaymentMethodComboBox.SelectedItem as PaymentMethod;
            float discount = (float)DiscountBox.Value;
            string paymentNotes = PaymentNotesTextBox.Text;

            try
            {
                // Update bill information
                bool success = await BillDAO.Instance.CheckOutAsync(
                    _currentBillId,
                    discount,
                    selectedPaymentMethod.Id,
                    _selectedGuest?.Id,
                    paymentNotes
                );

                if (success)
                {
                    // Update guest points if selected
                    if (_selectedGuest != null)
                    {
                        double finalAmount = _originalTotal * (100 - discount) / 100;
                        int pointsEarned = (int)(finalAmount / 10000);
                        //await GuestDAO.Instance.AddPointsAsync(_selectedGuest.Id, pointsEarned);
                    }

                    await ShowDialog("Thành công", $"Đã thanh toán cho bàn {_currentTable.Name}.");
                    CheckoutCompleted?.Invoke(this, true);
                    this.Close();
                }
                else
                {
                    await ShowDialog("Lỗi", "Không thể cập nhật trạng thái hóa đơn.");
                }
            }
            catch (Exception ex)
            {
                await ShowDialog("Lỗi", $"Đã xảy ra lỗi: {ex.Message}");
            }
        }

        private async Task ShowDialog(string title, string content)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = title,
                Content = content,
                CloseButtonText = "OK"
            };

            // Cài đặt XamlRoot từ rootElement
            FrameworkElement rootElement = this.Content as FrameworkElement;
            if (rootElement != null)
            {
                dialog.XamlRoot = rootElement.XamlRoot;
                await dialog.ShowAsync();
            }
        }
    }
}