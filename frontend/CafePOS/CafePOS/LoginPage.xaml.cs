using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using CafePOS.DAO;
using CafePOS.DTO;
using Windows.Storage;
using Windows.Security.Credentials;
using System.Security.Cryptography;
using System.Text;

namespace CafePOS
{
    public sealed partial class LoginPage : Page
    {
        private ApplicationDataContainer localSettings;
        private PasswordVault vault;
        private const string RESOURCE_NAME = "CafePOSCredentials";
        private const int TOKEN_EXPIRATION_DAYS = 30; // Token expires after 30 days

        public LoginPage()
        {
            this.InitializeComponent();

            // Initialize settings and vault
            localSettings = ApplicationData.Current.LocalSettings;
            vault = new PasswordVault();

            // Load saved credentials if they exist
            LoadSavedCredentials();
        }

        private async void LoadSavedCredentials()
        {
            try
            {
                if (localSettings.Values.ContainsKey("RememberLogin") &&
                    (bool)localSettings.Values["RememberLogin"] == true)
                {
                    // Check if token is expired
                    if (IsTokenExpired())
                    {
                        // Token expired, remove saved credentials
                        ClearSavedCredentials();
                        return;
                    }

                    // Get saved username
                    string username = null;
                    if (localSettings.Values.ContainsKey("Username"))
                    {
                        username = localSettings.Values["Username"].ToString();
                        UsernameBox.Text = username;
                    }

                    // Try to retrieve the token from secure storage
                    if (username != null)
                    {
                        try
                        {
                            var credential = vault.Retrieve(RESOURCE_NAME, username);
                            credential.RetrievePassword(); // This loads the password into the credential object

                            // Use the token to validate and automatically login
                            bool isTokenValid = await ValidateTokenAsync(username, credential.Password);

                            if (isTokenValid)
                            {
                                // Auto-login with token
                                await PerformTokenLoginAsync(username);
                                RememberMeCheckBox.IsChecked = true;
                            }
                            else
                            {
                                // Token is invalid, clear saved credentials
                                ClearSavedCredentials();
                            }
                        }
                        catch
                        {
                            // Credential not found or other vault error
                            ClearSavedCredentials();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading saved credentials: {ex.Message}");
            }
        }

        private bool IsTokenExpired()
        {
            if (localSettings.Values.ContainsKey("TokenExpiration"))
            {
                DateTime expirationDate = DateTime.Parse(localSettings.Values["TokenExpiration"].ToString());
                return DateTime.Now > expirationDate;
            }
            return true; // Assume expired if no expiration date is found
        }

        private string GenerateToken(string username, string password)
        {
            // Create a unique token based on username and a random value
            using (SHA256 sha256 = SHA256.Create())
            {
                // Combine username, password, and current timestamp for uniqueness
                string tokenBase = $"{username}|{password}|{DateTime.Now.Ticks}|{Guid.NewGuid()}";
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(tokenBase));

                // Convert to Base64 string for storage
                return Convert.ToBase64String(bytes);
            }
        }

        private async Task<bool> ValidateTokenAsync(string username, string token)
        {
            try
            {
                // In a real-world application, you would verify this token with your backend service
                // For this example, we'll just check if the account exists
                var userInfo = await AccountDAO.Instance.GetAccountByUserNameAsync(username);
                return userInfo != null;
            }
            catch
            {
                return false;
            }
        }

        private async Task PerformTokenLoginAsync(string username)
        {
            try
            {
                var userInfo = await AccountDAO.Instance.GetAccountByUserNameAsync(username);
                if (userInfo != null)
                {
                    int staffId = await StaffDAO.Instance.GetStaffIdByUserName(username);
                    Account.CurrentUserName = userInfo.UserName;
                    Account.CurrentDisplayName = userInfo.DisplayName;
                    Account.CurrentUserType = userInfo.Type;
                    Account.CurrentUserStaffId = staffId;

                    ErrorMessage.Visibility = Visibility.Collapsed;
                    if (this.Parent is Frame frame)
                    {
                        frame.Navigate(typeof(MainPage));
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in token login: {ex.Message}");
            }
        }

        private void SaveCredentialsWithToken(string username, string password)
        {
            try
            {
                // Generate a secure token
                string token = GenerateToken(username, password);

                // Save username in application settings
                localSettings.Values["Username"] = username;
                localSettings.Values["RememberLogin"] = true;

                // Set token expiration date
                DateTime expirationDate = DateTime.Now.AddDays(TOKEN_EXPIRATION_DAYS);
                localSettings.Values["TokenExpiration"] = expirationDate.ToString("o");

                // Save token in secure credential storage
                // First remove any existing credential for this user
                try
                {
                    var existingCredential = vault.Retrieve(RESOURCE_NAME, username);
                    vault.Remove(existingCredential);
                }
                catch { /* No existing credential */ }

                // Add the new credential
                vault.Add(new PasswordCredential(
                    RESOURCE_NAME,
                    username,
                    token
                ));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving credentials: {ex.Message}");
            }
        }

        private void ClearSavedCredentials()
        {
            try
            {
                // Clear from application settings
                if (localSettings.Values.ContainsKey("Username"))
                {
                    string username = localSettings.Values["Username"].ToString();

                    // Clear from secure storage
                    try
                    {
                        var credential = vault.Retrieve(RESOURCE_NAME, username);
                        vault.Remove(credential);
                    }
                    catch { /* Credential might not exist */ }
                }

                localSettings.Values.Remove("Username");
                localSettings.Values.Remove("TokenExpiration");
                localSettings.Values["RememberLogin"] = false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error clearing credentials: {ex.Message}");
            }
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
                    int staffId = await StaffDAO.Instance.GetStaffIdByUserName(username);
                    Account.CurrentUserName = userInfo.UserName;
                    Account.CurrentDisplayName = userInfo.DisplayName;
                    Account.CurrentUserType = userInfo.Type;
                    Account.CurrentUserStaffId = staffId;

                    // Handle "Remember Me" checkbox
                    if (RememberMeCheckBox.IsChecked == true)
                    {
                        SaveCredentialsWithToken(username, password);
                    }
                    else
                    {
                        ClearSavedCredentials();
                    }

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