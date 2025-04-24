                                                                                                                                                        using System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using Windows.UI.Xaml;
using CafePOS.DAO;
using CafePOS.DTO;
using Windows.Security.Credentials;
using Windows.Storage;


namespace CafePOS
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            UserNameText.Text = Account.CurrentDisplayName;
            UserRoleText.Text = Account.CurrentUserType == 1 ? "Quản lý" : "Nhân viên";


            // Set initial selection
            NavigateToPage("Dashboard");
            SetSelectedButton(DashboardButton);
        }

        private void NavigationButton_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;
            if (clickedButton != null)
            {
                string page = clickedButton.Tag.ToString();
                NavigateToPage(page);
                SetSelectedButton(clickedButton);
            }
        }

        private void NavigateToPage(string pageName)
        {
            // Update page title and description based on navigation
            switch (pageName)
            {
                case "Dashboard":
                    PageTitleText.Text = "Trang chủ";
                    PageDescriptionText.Text = "Tổng quan hệ thống quản lý";
                    // Navigate to dashboard page
                    // ContentFrame.Navigate(typeof(DashboardPage));
                    break;

                case "Orders":
                    PageTitleText.Text = "Quản lý đơn hàng";
                    PageDescriptionText.Text = "Tạo và quản lý các đơn hàng, theo dõi tình trạng";
                    // ContentFrame.Navigate(typeof(OrdersPage));
                    break;

                case "Tables":
                    PageTitleText.Text = "Quản lý bàn";
                    PageDescriptionText.Text = "Quản lý thanh toán các bàn";
                    ContentFrame.Navigate(typeof(TableManagerPage));
                    break;

                case "Product":
                    PageTitleText.Text = "Quản lý sản phẩm";
                    PageDescriptionText.Text = "Tạo và quản lý các sản phẩm";
                    ContentFrame.Navigate(typeof(ProductPage));
                    break;

                case "Inventory":
                    PageTitleText.Text = "Quản lý kho";
                    PageDescriptionText.Text = "Tạo và quản lý các nguyên liệu";
                    ContentFrame.Navigate(typeof(MaterialPage));
                    break;

                case "Staff":
                    if (Account.CurrentUserType != 1)
                    {
                        ContentDialog dialog = new ContentDialog
                        {
                            Title = "Không có quyền",
                            Content = "Bạn không có quyền truy cập trang quản lý nhân viên.",
                            CloseButtonText = "Đóng",
                            XamlRoot = this.XamlRoot
                        };
                        _ = dialog.ShowAsync();
                        return;
                    }

                    PageTitleText.Text = "Quản lý nhân viên";
                    PageDescriptionText.Text = "Lưu trữ thông tin nhân viên";
                    ContentFrame.Navigate(typeof(StaffPage));
                    break;

                case "Customers":
                    PageTitleText.Text = "Quản lý khách hàng";
                    PageDescriptionText.Text = "Lưu trữ thông tin khách hàng";
                    ContentFrame.Navigate(typeof(CustomersPage));
                    break;

                case "Payments":
                    PageTitleText.Text = "Phương thức thanh toán";
                    PageDescriptionText.Text = "Quản lý các phương thức thanh toán cho khách hàng";
                    ContentFrame.Navigate(typeof(PaymentMethodPage));
                    break;

                case "Invoices":
                    PageTitleText.Text = "Quản lý hoá đơn";
                    PageDescriptionText.Text = "Tra cứu và quản lý hoá đơn trong hệ thống";
                    ContentFrame.Navigate(typeof(BillManage));
                    break;

                case "Support":
                    PageTitleText.Text = "Hỗ trợ khách hàng";
                    PageDescriptionText.Text = "Giải đáp thắc mắc của khách hàng";
                    ContentFrame.Navigate(typeof(CustomerSupportPage));
                    break;

                case "Reports":
                    PageTitleText.Text = "Báo cáo bán hàng";
                    PageDescriptionText.Text = "Xem thống kê và báo cáo doanh thu";
                    ContentFrame.Navigate(typeof(StatisticPage));
                    break;
            }

            // Note: Uncomment the ContentFrame.Navigate lines when you have created the actual page classes
        }

        private void SetSelectedButton(Button selectedButton)
        {
            ResetButtonStates();

            VisualStateManager.GoToState(selectedButton, "Selected", true);
        }

        private void ResetButtonStates()
        {
            VisualStateManager.GoToState(DashboardButton, "Normal", true);
            VisualStateManager.GoToState(InventoryButton, "Normal", true);
            VisualStateManager.GoToState(StaffButton, "Normal", true);
            VisualStateManager.GoToState(CustomerButton, "Normal", true);
            VisualStateManager.GoToState(PaymentButton, "Normal", true);
            VisualStateManager.GoToState(InvoiceButton, "Normal", true);
            VisualStateManager.GoToState(SupportButton, "Normal", true);
            VisualStateManager.GoToState(ReportsButton, "Normal", true);
        }

        private async void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = "Xác nhận đăng xuất",
                Content = "Bạn có chắc chắn muốn đăng xuất khỏi hệ thống không?",
                PrimaryButtonText = "Đăng xuất",
                CloseButtonText = "Huỷ",
                XamlRoot = this.XamlRoot
            };

            ContentDialogResult result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                Account.CurrentUserName = null;
                Account.CurrentDisplayName = null;
                Account.CurrentUserType = 0;
                Account.CurrentUserStaffId = 0;
                var localSettings = ApplicationData.Current.LocalSettings;

                try
                {
                    string username = null;
                    if (localSettings.Values.ContainsKey("Username"))
                    {
                        username = localSettings.Values["Username"].ToString();
                    }

                    if (username != null)
                    {
                        try
                        {
                            var vault = new PasswordVault();
                            var credential = vault.Retrieve("CafePOSCredentials", username);
                            vault.Remove(credential);
                        }
                        catch
                        {
                            // Credential might not exist, which is fine
                        }
                    }

                    localSettings.Values.Remove("Username");
                    localSettings.Values.Remove("TokenExpiration");
                    localSettings.Values["RememberLogin"] = false;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error during logout: {ex.Message}");
                }

                Frame.Navigate(typeof(LoginPage));
            }
        }
    }
}