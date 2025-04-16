using System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using Windows.UI.Xaml;
using CafePOS.DAO;
using CafePOS.DTO;


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
                    PageDescriptionText.Text = "Tạo và quản lý các bàn";
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
                    PageDescriptionText.Text = "Quản lý thông tin nhân viên";
                    ContentFrame.Navigate(typeof(StaffPage));
                    break;

                case "Customers":
                    PageTitleText.Text = "Quản lý khách hàng";
                    PageDescriptionText.Text = "Lưu trữ thông tin và theo dõi lịch sử mua hàng";
                    // ContentFrame.Navigate(typeof(CustomersPage));
                    break;

                case "Payments":
                    PageTitleText.Text = "Phương thức thanh toán";
                    PageDescriptionText.Text = "Quản lý các phương thức thanh toán cho khách hàng";
                    // ContentFrame.Navigate(typeof(PaymentsPage));
                    break;

                case "Invoices":
                    PageTitleText.Text = "Quản lý hoá đơn";
                    PageDescriptionText.Text = "Tra cứu và quản lý hoá đơn trong hệ thống";
                    // ContentFrame.Navigate(typeof(InvoicesPage));
                    break;

                case "Support":
                    PageTitleText.Text = "Hỗ trợ khách hàng";
                    PageDescriptionText.Text = "Cung cấp dịch vụ chat và hỗ trợ trực tiếp";
                    // ContentFrame.Navigate(typeof(SupportPage));
                    break;

                case "Reports":
                    PageTitleText.Text = "Báo cáo bán hàng";
                    PageDescriptionText.Text = "Xem thống kê và báo cáo doanh thu";
                    // ContentFrame.Navigate(typeof(ReportsPage));
                    break;
            }

            // Note: Uncomment the ContentFrame.Navigate lines when you have created the actual page classes
        }

        private void SetSelectedButton(Button selectedButton)
        {
            // Reset all buttons to normal state
            ResetButtonStates();

            // Set the visual state for the selected button
            VisualStateManager.GoToState(selectedButton, "Selected", true);
        }

        private void ResetButtonStates()
        {
            // Reset all navigation buttons to normal state
            VisualStateManager.GoToState(DashboardButton, "Normal", true);
            VisualStateManager.GoToState(OrderButton, "Normal", true);
            VisualStateManager.GoToState(InventoryButton, "Normal", true);
            VisualStateManager.GoToState(StaffButton, "Normal", true);
            VisualStateManager.GoToState(CustomerButton, "Normal", true);
            VisualStateManager.GoToState(PaymentButton, "Normal", true);
            VisualStateManager.GoToState(InvoiceButton, "Normal", true);
            VisualStateManager.GoToState(SupportButton, "Normal", true);
            VisualStateManager.GoToState(ReportsButton, "Normal", true);
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            // Navigate back to login page
            Frame.Navigate(typeof(LoginPage));
        }
    }
}