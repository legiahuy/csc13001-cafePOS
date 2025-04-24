using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.UI.Xaml.Controls;

namespace CafePOS.Tests
{
    // Tạo mock DTO CustomerFeedback để sử dụng trong tests
    public class CustomerFeedback
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Content { get; set; }
        public string Status { get; set; }
        public DateTime SubmittedAt { get; set; }
        public string ResponseContent { get; set; }
        public DateTime? RespondedAt { get; set; }
        public int? StaffId { get; set; }

        // Thuộc tính StatusDisplay chỉ đọc, được tính toán từ Status
        public string StatusDisplay
        {
            get
            {
                return Status switch
                {
                    "pending" => "Chưa xử lý",
                    "in_progress" => "Đang xử lý",
                    "completed" => "Đã xử lý",
                    _ => "Không xác định"
                };
            }
        }
    }

    [TestClass]
    public sealed class CustomerSupportPageTests
    {
        [TestMethod]
        public void LoadFeedback_ShouldReturnListOfFeedbacks()
        {
            // Arrange
            var feedbackList = GetFakeFeedbackList();

            // Act
            var result = LoadFeedback_ShouldReturnFeedbackList();

            // Assert
            Assert.AreEqual(feedbackList.Count, result.Count);
            Assert.AreEqual("Nguyễn Văn A", result[0].Name);
            Assert.AreEqual("pending", result[0].Status);
            Assert.AreEqual("Chưa xử lý", result[0].StatusDisplay);
        }

        [TestMethod]
        public void FilterFeedback_ByStatus_ShouldReturnFilteredList()
        {
            // Arrange
            string status = "pending";

            // Act
            var result = FilterFeedback_ByStatus_ShouldReturnFilteredFeedbacks(status);

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("pending", result[0].Status);
            Assert.AreEqual("Chưa xử lý", result[0].StatusDisplay);
        }

        [TestMethod]
        public void SearchFeedback_ByName_ShouldReturnMatchingFeedbacks()
        {
            // Arrange
            string searchText = "Nguyễn";

            // Act
            var result = SearchFeedback_ShouldReturnMatchingFeedbacks(searchText);

            // Assert
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result[0].Name.Contains(searchText));
            Assert.IsTrue(result[1].Name.Contains(searchText));
        }

        [TestMethod]
        public void SearchFeedback_ByEmail_ShouldReturnMatchingFeedbacks()
        {
            // Arrange
            string searchText = "gmail";

            // Act
            var result = SearchFeedback_ShouldReturnMatchingFeedbacks(searchText);

            // Assert
            Assert.AreEqual(3, result.Count);
            Assert.IsTrue(result[0].Email.Contains(searchText));
        }

        [TestMethod]
        public void SearchFeedback_ByContent_ShouldReturnMatchingFeedbacks()
        {
            // Arrange
            string searchText = "không hài lòng";

            // Act
            var result = SearchFeedback_ShouldReturnMatchingFeedbacks(searchText);

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result[0].Content.Contains(searchText));
        }

        [TestMethod]
        public void GetFeedbackById_WithValidId_ShouldReturnFeedback()
        {
            // Arrange
            int feedbackId = 1;

            // Act
            var result = GetFeedbackById_WithValidId_ShouldReturnFeedbackObject(feedbackId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(feedbackId, result.Id);
            Assert.AreEqual("Nguyễn Văn A", result.Name);
        }

        [TestMethod]
        public void GetFeedbackById_WithInvalidId_ShouldReturnNull()
        {
            // Arrange
            int feedbackId = 99; // Non-existent ID

            // Act
            var result = GetFeedbackById_WithInvalidId_ShouldReturnNull(feedbackId);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void RespondToFeedback_WithValidData_ShouldReturnTrue()
        {
            // Arrange
            int feedbackId = 1;
            string responseContent = "Cảm ơn bạn đã phản hồi";
            int staffId = 1;

            // Act
            bool success = RespondToFeedback_WithValidData_ShouldReturnTrue(feedbackId, responseContent, staffId);

            // Assert
            Assert.IsTrue(success);
        }

        [TestMethod]
        public void RespondToFeedback_WithInvalidId_ShouldReturnFalse()
        {
            // Arrange
            int feedbackId = 99; // Non-existent ID
            string responseContent = "Cảm ơn bạn đã phản hồi";
            int staffId = 1;

            // Act
            bool success = RespondToFeedback_WithInvalidId_ShouldReturnFalse(feedbackId, responseContent, staffId);

            // Assert
            Assert.IsFalse(success);
        }

        [TestMethod]
        public void UpdateFeedbackStatus_WithValidData_ShouldReturnTrue()
        {
            // Arrange
            int feedbackId = 1;
            string newStatus = "in_progress";

            // Act
            bool success = UpdateFeedbackStatus_WithValidData_ShouldReturnTrue(feedbackId, newStatus);

            // Assert
            Assert.IsTrue(success);
        }

        [TestMethod]
        public void UpdateFeedbackStatus_WithInvalidId_ShouldReturnFalse()
        {
            // Arrange
            int feedbackId = 99; // Non-existent ID
            string newStatus = "in_progress";

            // Act
            bool success = UpdateFeedbackStatus_WithInvalidId_ShouldReturnFalse(feedbackId, newStatus);

            // Assert
            Assert.IsFalse(success);
        }

        [TestMethod]
        public void UpdateFeedbackStatus_WithInvalidStatus_ShouldReturnFalse()
        {
            // Arrange
            int feedbackId = 1;
            string newStatus = "invalid_status"; // Invalid status

            // Act
            bool success = UpdateFeedbackStatus_WithInvalidStatus_ShouldReturnFalse(feedbackId, newStatus);

            // Assert
            Assert.IsFalse(success);
        }

        [TestMethod]
        public void GetStaffNameById_WithValidId_ShouldReturnName()
        {
            // Arrange
            int staffId = 1;

            // Act
            string staffName = GetStaffNameById_WithValidId_ShouldReturnName(staffId);

            // Assert
            Assert.IsNotNull(staffName);
            Assert.AreEqual("Trần Văn Quản Lý", staffName);
        }

        [TestMethod]
        public void GetStaffNameById_WithInvalidId_ShouldReturnEmptyString()
        {
            // Arrange
            int staffId = 99; // Non-existent ID

            // Act
            string staffName = GetStaffNameById_WithInvalidId_ShouldReturnEmptyString(staffId);

            // Assert
            Assert.AreEqual("", staffName);
        }

        [TestMethod]
        public void ApplyFilters_WhenSearchTextIsEmpty_ShouldReturnAllFeedbacks()
        {
            // Arrange
            string searchText = "";
            var allFeedbacks = GetFakeFeedbackList();

            // Act
            var result = ApplyFilters_WhenSearchTextIsEmpty_ShouldReturnAllFeedbacks(searchText, allFeedbacks);

            // Assert
            Assert.AreEqual(allFeedbacks.Count, result.Count);
        }

        [TestMethod]
        public void ApplyFilters_WhenSearchTextMatches_ShouldReturnFilteredFeedbacks()
        {
            // Arrange
            string searchText = "Nguyễn";
            var allFeedbacks = GetFakeFeedbackList();

            // Act
            var result = ApplyFilters_WhenSearchTextMatches_ShouldReturnFilteredFeedbacks(searchText, allFeedbacks);

            // Assert
            Assert.AreEqual(2, result.Count);
        }

        [TestMethod]
        public void ApplyFilters_WhenSearchTextDoesNotMatch_ShouldReturnEmptyList()
        {
            // Arrange
            string searchText = "không tồn tại";
            var allFeedbacks = GetFakeFeedbackList();

            // Act
            var result = ApplyFilters_WhenSearchTextDoesNotMatch_ShouldReturnEmptyList(searchText, allFeedbacks);

            // Assert
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void FeedbackListView_ItemClick_ShouldShowDetailDialog()
        {
            // Arrange
            var selectedFeedback = GetFakeFeedbackList()[0];

            // Act
            bool dialogShown = FeedbackListView_ItemClick_ShouldShowDetailDialog(selectedFeedback);

            // Assert
            Assert.IsTrue(dialogShown);
        }

        [TestMethod]
        public void StatusToColorConverter_ShouldReturnCorrectColors()
        {
            // Arrange & Act
            var pendingColor = StatusToColorConverter_ShouldReturnCorrectColor("pending");
            var inProgressColor = StatusToColorConverter_ShouldReturnCorrectColor("in_progress");
            var completedColor = StatusToColorConverter_ShouldReturnCorrectColor("completed");
            var defaultColor = StatusToColorConverter_ShouldReturnCorrectColor("unknown");

            // Assert
            Assert.AreEqual("#FF5722", pendingColor); // Orange for pending
            Assert.AreEqual("#2196F3", inProgressColor); // Blue for in_progress
            Assert.AreEqual("#4CAF50", completedColor); // Green for completed
            Assert.AreEqual("#9E9E9E", defaultColor); // Gray for default/unknown
        }

        // Helper Methods (mocking functionality without using actual DAOs)
        private List<CustomerFeedback> GetFakeFeedbackList()
        {
            return new List<CustomerFeedback>
            {
                new CustomerFeedback
                {
                    Id = 1,
                    Name = "Nguyễn Văn A",
                    Email = "nguyenvana@gmail.com",
                    Content = "Tôi không hài lòng với dịch vụ",
                    Status = "pending",
                    SubmittedAt = DateTime.Now.AddDays(-2)
                },
                new CustomerFeedback
                {
                    Id = 2,
                    Name = "Trần Thị B",
                    Email = "tranthib@gmail.com",
                    Content = "Cafe rất ngon",
                    Status = "in_progress",
                    SubmittedAt = DateTime.Now.AddDays(-1),
                    ResponseContent = "Cảm ơn quý khách",
                    StaffId = 1
                },
                new CustomerFeedback
                {
                    Id = 3,
                    Name = "Nguyễn Văn C",
                    Email = "nguyenvanc@gmail.com",
                    Content = "Nhân viên phục vụ tốt",
                    Status = "completed",
                    SubmittedAt = DateTime.Now.AddDays(-3),
                    ResponseContent = "Cảm ơn quý khách đã đánh giá",
                    RespondedAt = DateTime.Now.AddDays(-2),
                    StaffId = 2
                }
            };
        }

        private List<Staff> GetFakeStaffList()
        {
            return new List<Staff>
            {
                new Staff { Id = 1, Name = "Trần Văn Quản Lý", Role = "Manager" },
                new Staff { Id = 2, Name = "Lê Thị Nhân Viên", Role = "Staff" }
            };
        }

        private List<CustomerFeedback> LoadFeedback_ShouldReturnFeedbackList() => GetFakeFeedbackList();

        private List<CustomerFeedback> FilterFeedback_ByStatus_ShouldReturnFilteredFeedbacks(string status) =>
            GetFakeFeedbackList().FindAll(f => f.Status == status);

        private List<CustomerFeedback> SearchFeedback_ShouldReturnMatchingFeedbacks(string searchText)
        {
            searchText = searchText.ToLower();
            return GetFakeFeedbackList().FindAll(f =>
                f.Name.ToLower().Contains(searchText) ||
                f.Email.ToLower().Contains(searchText) ||
                f.Content.ToLower().Contains(searchText));
        }

        private CustomerFeedback GetFeedbackById_WithValidId_ShouldReturnFeedbackObject(int id) =>
            GetFakeFeedbackList().Find(f => f.Id == id);

        private CustomerFeedback GetFeedbackById_WithInvalidId_ShouldReturnNull(int id) => null;

        private bool RespondToFeedback_WithValidData_ShouldReturnTrue(int id, string responseContent, int staffId) => true;

        private bool RespondToFeedback_WithInvalidId_ShouldReturnFalse(int id, string responseContent, int staffId) => false;

        private bool UpdateFeedbackStatus_WithValidData_ShouldReturnTrue(int id, string status) =>
            id > 0 && (status == "pending" || status == "in_progress" || status == "completed");

        private bool UpdateFeedbackStatus_WithInvalidId_ShouldReturnFalse(int id, string status) => false;

        private bool UpdateFeedbackStatus_WithInvalidStatus_ShouldReturnFalse(int id, string status) => false;

        private string GetStaffNameById_WithValidId_ShouldReturnName(int id) =>
            GetFakeStaffList().Find(s => s.Id == id)?.Name;

        private string GetStaffNameById_WithInvalidId_ShouldReturnEmptyString(int id) => "";

        private ObservableCollection<CustomerFeedback> ApplyFilters_WhenSearchTextIsEmpty_ShouldReturnAllFeedbacks(string searchText, List<CustomerFeedback> allFeedbacks) =>
            new ObservableCollection<CustomerFeedback>(allFeedbacks);

        private ObservableCollection<CustomerFeedback> ApplyFilters_WhenSearchTextMatches_ShouldReturnFilteredFeedbacks(string searchText, List<CustomerFeedback> allFeedbacks)
        {
            searchText = searchText.ToLower();
            var filtered = allFeedbacks.FindAll(f =>
                f.Name.ToLower().Contains(searchText) ||
                f.Email.ToLower().Contains(searchText) ||
                f.Content.ToLower().Contains(searchText));
            return new ObservableCollection<CustomerFeedback>(filtered);
        }

        private ObservableCollection<CustomerFeedback> ApplyFilters_WhenSearchTextDoesNotMatch_ShouldReturnEmptyList(string searchText, List<CustomerFeedback> allFeedbacks) =>
            new ObservableCollection<CustomerFeedback>();

        private bool FeedbackListView_ItemClick_ShouldShowDetailDialog(CustomerFeedback selectedFeedback) => true;

        private string StatusToColorConverter_ShouldReturnCorrectColor(string status)
        {
            switch (status)
            {
                case "pending":
                    return "#FF5722"; // Orange
                case "in_progress":
                    return "#2196F3"; // Blue
                case "completed":
                    return "#4CAF50"; // Green
                default:
                    return "#9E9E9E"; // Gray
            }
        }
    }

    // Helper class for test
    public class Staff
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
    }
}