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
using System.IO;


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
                UpdatePrices();
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
                UpdatePrices();
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

        private void UpdatePrices()
        {
            if (TotalPriceTextBlock != null)
            {
                TotalPriceTextBlock.Text = _originalTotal.ToString("C0", _culture);
            }

            double finalAmount = _originalTotal;
            double discountFromPoints = 0;

            if (_selectedGuest != null && UsePointsPanel.Visibility == Visibility.Visible)
            {
                int pointsToUse = (int)UsePointsBox.Value;
                discountFromPoints = (pointsToUse / 50) * 10000;
                finalAmount -= discountFromPoints;
            }

            // Update Discount Text
            if (DiscountFromPointsTextBlock != null)
            {
                if (discountFromPoints > 0)
                {
                    DiscountFromPointsTextBlock.Visibility = Visibility.Visible;
                    DiscountFromPointsTextBlock.Text = $"- {discountFromPoints.ToString("N0", _culture)} ₫";
                }
                else
                {
                    DiscountFromPointsTextBlock.Visibility = Visibility.Collapsed;
                }
            }

            if (FinalPriceTextBlock != null)
            {
                FinalPriceTextBlock.Text = Math.Max(finalAmount, 0).ToString("C0", _culture);
            }

            if (_selectedGuest != null && PointsEarnedTextBlock != null)
            {
                int pointsEarned = (int)(_originalTotal / 10000);
                PointsEarnedTextBlock.Text = $"Điểm tích lũy từ hóa đơn này: {pointsEarned}";
            }
        }



        private void UsePointsBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            int pointsToUse = (int)sender.Value;

            // Validate luôn khi thay đổi
            if (pointsToUse > 0 && (pointsToUse < 50 || pointsToUse % 50 != 0))
            {
                _ = ShowDialog("Lưu ý", "Bạn cần nhập bội số của 50 điểm để đổi!");
                sender.Value = 0;
                return;
            }

            UpdatePrices();
        }
        private async void UsePointsBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                int pointsToUse = (int)UsePointsBox.Value;

                if (pointsToUse == 0)
                    return;

                double discountFromPoints = (pointsToUse / 50) * 10000;
                string formattedDiscount = discountFromPoints.ToString("C0", _culture);

                ContentDialog confirmDialog = new ContentDialog
                {
                    Title = "Xác nhận đổi điểm",
                    Content = $"Bạn muốn đổi {pointsToUse} điểm để giảm {formattedDiscount}?",
                    PrimaryButtonText = "Đồng ý",
                    CloseButtonText = "Hủy",
                };

                FrameworkElement rootElement = this.Content as FrameworkElement;
                if (rootElement != null)
                {
                    confirmDialog.XamlRoot = rootElement.XamlRoot;
                }

                var result = await confirmDialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    DiscountFromPointsTextBlock.Text = $"Giảm từ đổi điểm: {formattedDiscount}";
                    DiscountFromPointsTextBlock.Visibility = Visibility.Visible;
                    UpdatePrices();
                }
                else
                {
                    UsePointsBox.Value = 0;
                    DiscountFromPointsTextBlock.Visibility = Visibility.Collapsed;
                    UpdatePrices();
                }
            }
        }


        private void GuestSearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                string query = sender.Text.ToLower();
                var suggestions = _guestList
                    .Where(g => g.Name.ToLower().Contains(query) || (g.Phone != null && g.Phone.Contains(query)))
                    .ToList();

                sender.ItemsSource = suggestions;
            }
        }

        private void GuestSearchBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            if (args.SelectedItem is Guest guest)
            {
                SelectGuest(guest);
            }
        }

        private void GuestSearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion is Guest guest)
            {
                SelectGuest(guest);
            }
        }


        private void SelectGuest(Guest guest)
        {
            _selectedGuest = guest;
            GuestNameTextBlock.Text = guest.Name;
            GuestPhoneTextBlock.Text = guest.Phone ?? "Không có SĐT";
            GuestPointsTextBlock.Text = $"Điểm: {guest.TotalPoints}";
            GuestAvailablePointsTextBlock.Text = $"Điểm khả dụng: {guest.AvailablePoints}";
            GuestLevelTextBlock.Text = $"Hạng: {Guest.GetMembershipLevel(guest.TotalPoints)}";

            // Nếu khách đủ 50 điểm thì hiện box nhập UsePoints
            if (guest.AvailablePoints >= 50)
            {
                UsePointsPanel.Visibility = Visibility.Visible;
                UsePointsBox.Maximum = guest.AvailablePoints;
                UsePointsBox.Value = 0;
            }
            else
            {
                UsePointsPanel.Visibility = Visibility.Collapsed;
                UsePointsBox.Value = 0;
            }

            SelectedGuestPanel.Visibility = Visibility.Visible;
            GuestSearchBox.Text = guest.Name;

            UpdatePrices();
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
                        TotalPoints = 0,
                        AvailablePoints = 0,
                        MembershipLevel = Guest.GetMembershipLevel(0),
                        MemberSince = DateTime.Now
                    };

                    int guestId = await GuestDAO.Instance.AddGuestAsync(name, phone, email, notes, 0, 0, "Regular", DateOnly.FromDateTime(DateTime.Now));
                    
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

            // ⚡ Thêm check: nếu đang nhập điểm nhưng chưa xác nhận (DiscountFromPointsTextBlock bị ẩn)
            if (UsePointsPanel.Visibility == Visibility.Visible && UsePointsBox.Value > 0 && DiscountFromPointsTextBlock.Visibility == Visibility.Collapsed)
            {
                await ShowDialog("Thông báo", "Bạn cần nhấn Enter để xác nhận đổi điểm trước khi thanh toán.");
                return;
            }

            var selectedPaymentMethod = PaymentMethodComboBox.SelectedItem as PaymentMethod;
            string paymentNotes = PaymentNotesTextBox.Text;

            try
            {
                double finalAmount = _originalTotal;
                int pointsUsed = 0;

                if (_selectedGuest != null && UsePointsPanel.Visibility == Visibility.Visible)
                {
                    int pointsToUse = (int)UsePointsBox.Value;
                    int redeemablePoints = Math.Min(pointsToUse, _selectedGuest.AvailablePoints);
                    int redeemableUnits = redeemablePoints / 50;
                    double discountByPoints = redeemableUnits * 10000;

                    if (discountByPoints > finalAmount)
                    {
                        redeemableUnits = (int)(finalAmount / 10000);
                        discountByPoints = redeemableUnits * 10000;
                    }

                    pointsUsed = redeemableUnits * 50;
                    finalAmount -= discountByPoints;
                }

                bool success = await BillDAO.Instance.CheckOutAsync(
                     _currentBillId,
                     _originalTotal,
                     finalAmount,
                     selectedPaymentMethod.Id,
                     _selectedGuest?.Id,
                     paymentNotes,
                     Account.CurrentUserStaffId    // 🆕 ID của Staff đang đăng nhập
                 );



                if (success)
                {
                    if (_selectedGuest != null)
                    {
                        int pointsEarned = (int)(_originalTotal / 10000);
                        await GuestDAO.Instance.AddPointsAsync(_selectedGuest.Id, pointsEarned);

                        if (pointsUsed > 0)
                        {
                            await GuestDAO.Instance.DeductAvailablePointsAsync(_selectedGuest.Id, pointsUsed);
                        }
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
        private async void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var pdf = new PdfSharpCore.Pdf.PdfDocument();
                var page = pdf.AddPage();
                var gfx = PdfSharpCore.Drawing.XGraphics.FromPdfPage(page);
                var fontRegular = new PdfSharpCore.Drawing.XFont("Arial", 12, PdfSharpCore.Drawing.XFontStyle.Regular);
                var fontBold = new PdfSharpCore.Drawing.XFont("Arial", 14, PdfSharpCore.Drawing.XFontStyle.Bold);

                int y = 20;
                int pageWidth = (int)page.Width;

                // Header
                gfx.DrawString("CHẠM CAFÉ", new PdfSharpCore.Drawing.XFont("Arial", 20, PdfSharpCore.Drawing.XFontStyle.Bold),
                    PdfSharpCore.Drawing.XBrushes.Black, new PdfSharpCore.Drawing.XPoint(pageWidth / 2, y),
                    PdfSharpCore.Drawing.XStringFormats.Center);

                y += 30;
                gfx.DrawString("18 Tô Hiến Thành, Quận 10, TP.Hồ Chí Minh", fontRegular,
                    PdfSharpCore.Drawing.XBrushes.Black, new PdfSharpCore.Drawing.XPoint(pageWidth / 2, y),
                    PdfSharpCore.Drawing.XStringFormats.Center);

                y += 30;

                // Bill Info
                gfx.DrawString($"Số: {_currentBillId}", fontBold, PdfSharpCore.Drawing.XBrushes.Black, new PdfSharpCore.Drawing.XPoint(pageWidth / 2, y),
                    PdfSharpCore.Drawing.XStringFormats.Center);
                y += 20;
                gfx.DrawString($"Thời gian: {DateTime.Now:dd.MM.yyyy HH:mm}", fontRegular, PdfSharpCore.Drawing.XBrushes.Black, new PdfSharpCore.Drawing.XPoint(20, y));
                y += 20;
                gfx.DrawString($"Thu ngân: {Account.CurrentDisplayName}", fontRegular, PdfSharpCore.Drawing.XBrushes.Black, new PdfSharpCore.Drawing.XPoint(20, y));
                y += 20;
                gfx.DrawString($"{_currentTable?.Name}", fontRegular, PdfSharpCore.Drawing.XBrushes.Black, new PdfSharpCore.Drawing.XPoint(20, y));
                y += 20;

                if (_selectedGuest != null)
                {
                    gfx.DrawString($"Khách hàng: {_selectedGuest.Name}", fontRegular, PdfSharpCore.Drawing.XBrushes.Black, new PdfSharpCore.Drawing.XPoint(20, y));
                    y += 20;
                    gfx.DrawString($"Hạng: {Guest.GetMembershipLevel(_selectedGuest.TotalPoints)}", fontRegular, PdfSharpCore.Drawing.XBrushes.Black, new PdfSharpCore.Drawing.XPoint(20, y));
                    y += 20;
                }

                y += 10;
                gfx.DrawLine(PdfSharpCore.Drawing.XPens.Black, 20, y, pageWidth - 20, y); // Line ngay sau header
                y += 20;

                // Cột bảng món
                int xTT = 40;
                int xProductName = 100;
                int xSL = 260;
                int xPrice = 370;
                int xTotalPrice = 500;

                // Tiêu đề cột
                gfx.DrawString("STT", fontBold, PdfSharpCore.Drawing.XBrushes.Black, new PdfSharpCore.Drawing.XPoint(xTT, y));
                gfx.DrawString("Tên món", fontBold, PdfSharpCore.Drawing.XBrushes.Black, new PdfSharpCore.Drawing.XPoint(xProductName, y));
                gfx.DrawString("SL", fontBold, PdfSharpCore.Drawing.XBrushes.Black, new PdfSharpCore.Drawing.XPoint(xSL, y));
                gfx.DrawString("Đơn giá", fontBold, PdfSharpCore.Drawing.XBrushes.Black, new PdfSharpCore.Drawing.XPoint(xPrice, y));
                gfx.DrawString("T.Tiền", fontBold, PdfSharpCore.Drawing.XBrushes.Black, new PdfSharpCore.Drawing.XPoint(xTotalPrice, y));

                y += 20;
                gfx.DrawLine(PdfSharpCore.Drawing.XPens.Black, 20, y, pageWidth - 20, y); // Line sau tiêu đề cột
                y += 20;

                // Danh sách món
                int stt = 1;
                foreach (var item in _orderItems)
                {
                    gfx.DrawString($"{stt}", fontRegular, PdfSharpCore.Drawing.XBrushes.Black, new PdfSharpCore.Drawing.XPoint(xTT + 5, y));
                    gfx.DrawString($"{item.ProductName}", fontRegular, PdfSharpCore.Drawing.XBrushes.Black, new PdfSharpCore.Drawing.XPoint(xProductName + 5, y));
                    gfx.DrawString($"{item.Count}", fontRegular, PdfSharpCore.Drawing.XBrushes.Black, new PdfSharpCore.Drawing.XPoint(xSL + 5, y));
                    gfx.DrawString($"{item.Price:N0}", fontRegular, PdfSharpCore.Drawing.XBrushes.Black, new PdfSharpCore.Drawing.XPoint(xPrice + 5, y));
                    gfx.DrawString($"{item.TotalPrice:N0}", fontRegular, PdfSharpCore.Drawing.XBrushes.Black, new PdfSharpCore.Drawing.XPoint(xTotalPrice + 5, y));
                    y += 20;
                    stt++;
                }

                y += 5;
                gfx.DrawLine(PdfSharpCore.Drawing.XPens.Black, 20, y, pageWidth - 20, y); // Line sau danh sách món
                y += 20;

                // Tổng cộng
                gfx.DrawString("Tổng số lượng:", fontBold, PdfSharpCore.Drawing.XBrushes.Black, new PdfSharpCore.Drawing.XPoint(20, y));
                gfx.DrawString($"{_orderItems.Sum(i => i.Count)}", fontBold, PdfSharpCore.Drawing.XBrushes.Black, new PdfSharpCore.Drawing.XPoint(xSL + 5, y));
                y += 20;

                // Sau phần in "Thành tiền" và trước "Thanh toán":

                gfx.DrawString("Thành tiền:", fontBold, PdfSharpCore.Drawing.XBrushes.Black, new PdfSharpCore.Drawing.XPoint(20, y));
                gfx.DrawString($"{_originalTotal.ToString("N0", _culture)} ₫", fontBold, PdfSharpCore.Drawing.XBrushes.Black, new PdfSharpCore.Drawing.XPoint(xTotalPrice, y));
                y += 20;

                // ➡️ Nếu có giảm giá từ đổi điểm thì in thêm dòng
                if (UsePointsPanel.Visibility == Visibility.Visible && UsePointsBox.Value > 0)
                {
                    double discountFromPoints = (UsePointsBox.Value / 50) * 10000;

                    gfx.DrawString("Giảm từ đổi điểm:", fontBold, PdfSharpCore.Drawing.XBrushes.Black, new PdfSharpCore.Drawing.XPoint(20, y));
                    gfx.DrawString($"-{discountFromPoints:N0} ₫", fontBold, PdfSharpCore.Drawing.XBrushes.Black, new PdfSharpCore.Drawing.XPoint(xTotalPrice, y));
                    y += 20;
                }

                // Sau đó in "Thanh toán" như bình thường
                gfx.DrawString("Thanh toán:", fontBold, PdfSharpCore.Drawing.XBrushes.Black, new PdfSharpCore.Drawing.XPoint(20, y));
                gfx.DrawString($"{FinalPriceTextBlock.Text}", fontBold, PdfSharpCore.Drawing.XBrushes.Black, new PdfSharpCore.Drawing.XPoint(xTotalPrice, y));
                y += 20;


                y += 5;
                gfx.DrawLine(PdfSharpCore.Drawing.XPens.Black, 20, y, pageWidth - 20, y); // Line sau tổng cộng
                y += 10;

                // Footer
                gfx.DrawString("Giá sản phẩm đã bao gồm VAT 8%.", fontRegular, PdfSharpCore.Drawing.XBrushes.Black, new PdfSharpCore.Drawing.XPoint(pageWidth / 2, y), PdfSharpCore.Drawing.XStringFormats.Center);
                y += 20;
                gfx.DrawString("Nếu bạn cần xuất hóa đơn, hãy liên hệ cửa hàng.", fontRegular, PdfSharpCore.Drawing.XBrushes.Black, new PdfSharpCore.Drawing.XPoint(pageWidth / 2, y), PdfSharpCore.Drawing.XStringFormats.Center);
                y += 20;
                gfx.DrawString("Mọi thắc mắc xin liên hệ: 02871 087 088.", fontRegular, PdfSharpCore.Drawing.XBrushes.Black, new PdfSharpCore.Drawing.XPoint(pageWidth / 2, y), PdfSharpCore.Drawing.XStringFormats.Center);
                y += 20;
                gfx.DrawString("Password Wifi: touchthetaste", fontBold, PdfSharpCore.Drawing.XBrushes.Black, new PdfSharpCore.Drawing.XPoint(pageWidth / 2, y), PdfSharpCore.Drawing.XStringFormats.Center);

                // Save file
                var folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var filePath = Path.Combine(folder, $"Bill_{_currentBillId}.pdf");
                using (var stream = File.Create(filePath))
                {
                    pdf.Save(stream, false);
                }

                await ShowDialog("Thành công", $"Đã in hóa đơn ra file:\n{filePath}");
            }
            catch (Exception ex)
            {
                await ShowDialog("Lỗi", ex.Message);
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