using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using CafePOS.DAO;
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

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace CafePOS
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        public LoginPage()
        {
            this.InitializeComponent();
        }
        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameBox.Text;
            string password = PasswordBox.Password;
            LoginButton.IsEnabled = false;

            try
            {
                bool isLoginSuccessful = await AccountDAO.Instance.LoginAsync(username, password);

                if (!isLoginSuccessful)
                {
                    ErrorMessage.Text = "Sai tài khoản hoặc mật khẩu!";
                    ErrorMessage.Visibility = Visibility.Visible;
                }
                else
                {
                    var userInfo = await AccountDAO.Instance.GetAccountByUserNameAsync(username);

                    DataProvider.CurrentUserName = userInfo.UserName;
                    DataProvider.CurrentDisplayName = userInfo.DisplayName;
                    DataProvider.CurrentUserType = userInfo.Type;
                    ErrorMessage.Visibility = Visibility.Collapsed;
                    if (this.Parent is Frame frame)
                    {
                        frame.Navigate(typeof(MainPage));
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.Text = "Đã xảy ra lỗi. Vui lòng thử lại.";
                ErrorMessage.Visibility = Visibility.Visible;
            }
            finally
            {
                LoginButton.IsEnabled = true;
            }
        }
    }


}
