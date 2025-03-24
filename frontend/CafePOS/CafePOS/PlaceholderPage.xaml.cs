using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;

namespace CafePOS
{
    /// <summary>
    /// A placeholder page that displays when a feature is still under development.
    /// </summary>
    public sealed partial class PlaceholderPage : Page
    {
        public PlaceholderPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // If you want to display which page was navigated to
            if (e.Parameter is string pageName)
            {
                // Update UI based on which page was requested
                string pageTitle = GetPageTitle(pageName);
                PageNameText.Text = $"{pageTitle} - Đang phát triển";
            }
        }

        private string GetPageTitle(string pageTag)
        {
            // Convert page tag to human-readable title
            switch (pageTag)
            {
                case "dashboard":
                    return "Trang chủ";
                case "orders":
                    return "Quản lý đơn hàng";
                case "inventory":
                    return "Quản lý kho";
                case "staff":
                    return "Quản lý nhân viên";
                case "customers":
                    return "Quản lý khách hàng";
                case "payments":
                    return "Phương thức thanh toán";
                case "invoices":
                    return "Quản lý hoá đơn";
                case "support":
                    return "Hỗ trợ khách hàng";
                case "reports":
                    return "Báo cáo bán hàng";
                default:
                    return "Trang";
            }
        }
    }
}