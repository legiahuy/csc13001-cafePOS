using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CafePOS.DAO;
using CafePOS.DTO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Linq;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace CafePOS
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CustomerSupportPage : Page
    {
        private string currentSearchText = "";
        private ObservableCollection<CustomerFeedback> allFeedbackItems;
        private ObservableCollection<CustomerFeedback> FeedbackItems { get; set; }

        public CustomerSupportPage()
        {
            this.InitializeComponent();
            FeedbackItems = new ObservableCollection<CustomerFeedback>();
            Loaded += FeedbackPage_Loaded;
        }

        private async void FeedbackPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadFeedback();
        }

        private async Task LoadFeedback()
        {
            try
            {
                string? status = "all";

                if (StatusFilter.SelectedItem != null)
                {
                    status = ((ComboBoxItem)StatusFilter.SelectedItem).Tag?.ToString();
                }

                var feedbacks = status!.Equals("all")
                    ? await CustomerFeedbackDAO.Instance.GetAllFeedbacksWithoutFilterAsync()
                    : await CustomerFeedbackDAO.Instance.GetAllFeedbacksAsync(status!);

                // Store all feedbacks
                allFeedbackItems = new ObservableCollection<CustomerFeedback>(feedbacks);

                // Apply search filter if there's search text
                ApplyFilters();
            }
            catch (Exception ex)
            {
                await ShowErrorDialog("Lỗi", $"Không thể tải dữ liệu: {ex.Message}");
            }
        }

        private async void StatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await LoadFeedback();
        }

        private async void FeedbackListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is not CustomerFeedback selectedFeedback)
            {
                await ShowErrorDialog("Lỗi", "Không thể tải thông tin phản hồi");
                return;
            }

            var feedbackDetail = await CustomerFeedbackDAO.Instance.GetFeedbackByIdAsync(selectedFeedback.Id);

            if (feedbackDetail == null)
            {
                await ShowErrorDialog("Lỗi", "Không thể tải thông tin chi tiết phản hồi");
                return;
            }

            // Show feedback detail dialog
            ContentDialog detailDialog = new ContentDialog
            {
                Title = $"Phản hồi từ {feedbackDetail.Name}",
                PrimaryButtonText = "Phản hồi",
                CloseButtonText = "Đóng",
                XamlRoot = this.XamlRoot
            };

            // Create dialog content
            StackPanel panel = new StackPanel { Spacing = 10 };

            // Validate and add email
            if (!string.IsNullOrWhiteSpace(feedbackDetail.Email) && IsValidEmail(feedbackDetail.Email))
            {
                panel.Children.Add(new TextBlock
                {
                    Text = $"Email: {feedbackDetail.Email}",
                    TextWrapping = TextWrapping.Wrap
                });
            }

            // Add submission time
            panel.Children.Add(new TextBlock
            {
                Text = $"Thời gian: {feedbackDetail.SubmittedAt:dd/MM/yyyy HH:mm}",
                TextWrapping = TextWrapping.Wrap
            });

            // Add status
            panel.Children.Add(new TextBlock
            {
                Text = $"Trạng thái: {feedbackDetail.StatusDisplay}",
                TextWrapping = TextWrapping.Wrap
            });

            // Add feedback content
            panel.Children.Add(new TextBlock
            {
                Text = "Nội dung phản hồi:",
                FontWeight = Microsoft.UI.Text.FontWeights.Bold
            });

            panel.Children.Add(new TextBlock
            {
                Text = feedbackDetail.Content,
                TextWrapping = TextWrapping.Wrap
            });

            // Add response if exists
            if (!string.IsNullOrEmpty(feedbackDetail.ResponseContent))
            {
                panel.Children.Add(new TextBlock
                {
                    Text = "Phản hồi của cửa hàng:",
                    FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                    Margin = new Thickness(0, 10, 0, 0)
                });

                panel.Children.Add(new TextBlock
                {
                    Text = feedbackDetail.ResponseContent,
                    TextWrapping = TextWrapping.Wrap
                });

                if (feedbackDetail.RespondedAt.HasValue)
                {
                    panel.Children.Add(new TextBlock
                    {
                        Text = $"Phản hồi lúc: {feedbackDetail.RespondedAt.Value:dd/MM/yyyy HH:mm}",
                        TextWrapping = TextWrapping.Wrap,
                        FontSize = 12,
                        Opacity = 0.7
                    });
                }

                // Add staff information if available
                if (feedbackDetail.StaffId > 0)
                {
                    int id = (int)feedbackDetail.StaffId;
                    var staffName = await StaffDAO.Instance.GetStaffNameById(id);
                    if (!string.IsNullOrWhiteSpace(staffName))
                    {
                        panel.Children.Add(new TextBlock
                        {
                            Text = $"Nhân viên phản hồi: {staffName} (ID: {feedbackDetail.StaffId})",
                            TextWrapping = TextWrapping.Wrap,
                            FontSize = 12,
                            Opacity = 0.7,
                            Margin = new Thickness(0, 5, 0, 0)
                        });
                    }
                }
                else if (feedbackDetail.StaffId == null)
                {
                    panel.Children.Add(new TextBlock
                    {
                        Text = $"Phản hồi bởi quản trị viên",
                        TextWrapping = TextWrapping.Wrap,
                        FontSize = 12,
                        Opacity = 0.7,
                        Margin = new Thickness(0, 5, 0, 0)
                    });
                }
            }

            detailDialog.Content = panel;

            var result = await detailDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                await ShowResponseDialog(feedbackDetail);
            }
        }

        private async Task ShowResponseDialog(CustomerFeedback feedback)
        {
            // Validate feedback status
            if (feedback.Status == "completed")
            {
                await ShowErrorDialog("Lỗi", "Phản hồi này đã được xử lý xong!");
                return;
            }

            ContentDialog responseDialog = new ContentDialog
            {
                Title = $"Phản hồi cho {feedback.Name}",
                PrimaryButtonText = "Gửi",
                SecondaryButtonText = "Đánh dấu đang xử lý",
                CloseButtonText = "Hủy",
                XamlRoot = this.XamlRoot
            };

            StackPanel panel = new StackPanel { Spacing = 10 };

            TextBox responseTextBox = new TextBox
            {
                PlaceholderText = "Nhập nội dung phản hồi",
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true,
                Height = 150
            };

            if (!string.IsNullOrEmpty(feedback.ResponseContent))
            {
                responseTextBox.Text = feedback.ResponseContent;
            }

            panel.Children.Add(responseTextBox);
            responseDialog.Content = panel;

            var result = await responseDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                string responseText = responseTextBox.Text?.Trim() ?? "";

                // Validate empty response
                if (string.IsNullOrWhiteSpace(responseText))
                {
                    await ShowErrorDialog("Lỗi", "Vui lòng nhập nội dung phản hồi!");
                    return;
                }

                // Validate response length
                if (responseText.Length > 1000)
                {
                    await ShowErrorDialog("Lỗi", "Nội dung phản hồi không được vượt quá 1000 ký tự!");
                    return;
                }

                int currentStaffId = Account.CurrentUserStaffId;
                bool success = await CustomerFeedbackDAO.Instance.RespondToFeedbackAsync(
                    feedback.Id,
                    responseText,
                    currentStaffId > 0 ? currentStaffId : null
                );

                if (success)
                {
                    await ShowSuccessDialog("Thành công", "Đã gửi phản hồi thành công!");
                    await LoadFeedback();
                }
                else
                {
                    await ShowErrorDialog("Lỗi", "Không thể gửi phản hồi. Vui lòng thử lại!");
                }
            }
            else if (result == ContentDialogResult.Secondary)
            {
                // Mark as in progress
                bool success = await CustomerFeedbackDAO.Instance.UpdateFeedbackStatusAsync(
                    feedback.Id,
                    "in_progress"
                );

                if (success)
                {
                    await ShowSuccessDialog("Thành công", "Đã đánh dấu phản hồi đang xử lý!");
                    await LoadFeedback();
                }
                else
                {
                    await ShowErrorDialog("Lỗi", "Không thể cập nhật trạng thái. Vui lòng thử lại!");
                }
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private async Task ShowErrorDialog(string title, string content)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = content,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
        }

        private async Task ShowSuccessDialog(string title, string content)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = content,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
        }

        private void ApplyFilters()
        {
            if (allFeedbackItems == null)
                return;

            FeedbackItems.Clear();

            // If search text is empty, show all items
            if (string.IsNullOrWhiteSpace(currentSearchText))
            {
                foreach (var item in allFeedbackItems)
                {
                    FeedbackItems.Add(item);
                }
            }
            else
            {
                // Filter by search text (case insensitive)
                string searchLower = currentSearchText.ToLower();
                foreach (var item in allFeedbackItems)
                {
                    if (item.Name?.ToLower().Contains(searchLower) == true ||
                        item.Email?.ToLower().Contains(searchLower) == true ||
                        item.Content?.ToLower().Contains(searchLower) == true)
                    {
                        FeedbackItems.Add(item);
                    }
                }
            }

            FeedbackListView.ItemsSource = FeedbackItems;
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            currentSearchText = SearchBox.Text;
            ApplyFilters();
        }

        private void SearchBox_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                // Focus on the list view after pressing Enter
                FeedbackListView.Focus(FocusState.Programmatic);
            }
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            SearchBox.Text = "";
            currentSearchText = "";
            await LoadFeedback();
        }
    }
}
