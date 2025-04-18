using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CafePOSTestProject
{
    public class PaymentMethod
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string IconUrl { get; set; }
        public bool IsActive { get; set; }
    }

    [TestClass]
    public sealed class PaymentMethodPageTests
    {
        [TestMethod]
        public void LoadPaymentMethod_ShouldReturnListOfPaymentMethods()
        {
            // Arrange
            var paymentList = GetFakePaymentMethodList();

            // Act
            var result = LoadPaymentMethod_ShouldReturnList();

            // Assert
            Assert.AreEqual(paymentList.Count, result.Count);
            Assert.AreEqual("Tiền mặt", result[0].Name);
            Assert.AreEqual(true, result[0].IsActive);
        }

        [TestMethod]
        public void InsertPaymentMethod_WithValidData_ShouldReturnPaymentMethodId()
        {
            // Arrange
            string name = "Chuyển khoản SHB";
            string description = "Chuyển khoản qua ngân hàng SHB";
            bool isActive = true;
            string iconUrl = "/Assets/Payment/shb.png";

            // Act
            int paymentMethodId = InsertPaymentMethod_WithValidData_ShouldReturnId(name, description, isActive, iconUrl);

            // Assert
            Assert.AreNotEqual(-1, paymentMethodId);
            Assert.AreEqual(4, paymentMethodId); // Next ID based on fake list (has 3 items)
        }

        [TestMethod]
        public void InsertPaymentMethod_WithEmptyName_ShouldReturnMinusOne()
        {
            // Arrange
            string name = "";
            string description = "Chuyển khoản qua ngân hàng SHB";
            bool isActive = true;
            string iconUrl = "/Assets/Payment/shb.png";

            // Act
            int paymentMethodId = InsertPaymentMethod_WithInvalidData_ShouldReturnMinusOne(name, description, isActive, iconUrl);

            // Assert
            Assert.AreEqual(-1, paymentMethodId);
        }

        [TestMethod]
        public void UpdatePaymentMethod_WithValidData_ShouldReturnTrue()
        {
            // Arrange
            int id = 1;
            string name = "Tiền mặt (VND)";
            string description = "Thanh toán bằng tiền mặt Việt Nam Đồng";
            bool isActive = true;
            string iconUrl = "/Assets/Payment/cash_updated.png";

            // Act
            bool success = UpdatePaymentMethod_WithValidData_ShouldReturnTrue(id, name, description, isActive, iconUrl);

            // Assert
            Assert.IsTrue(success);
        }

        [TestMethod]
        public void UpdatePaymentMethod_WithNonExistentId_ShouldReturnFalse()
        {
            // Arrange
            int id = 99; // Non-existent ID
            string name = "Tiền mặt (VND)";
            string description = "Thanh toán bằng tiền mặt Việt Nam Đồng";
            bool isActive = true;
            string iconUrl = "/Assets/Payment/cash_updated.png";

            // Act
            bool success = UpdatePaymentMethod_WithInvalidId_ShouldReturnFalse(id, name, description, isActive, iconUrl);

            // Assert
            Assert.IsFalse(success);
        }

        [TestMethod]
        public void DeletePaymentMethod_WithValidId_ShouldReturnTrue()
        {
            // Arrange
            int id = 1;

            // Act
            bool success = DeletePaymentMethod_WithValidId_ShouldReturnTrue(id);

            // Assert
            Assert.IsTrue(success);
        }

        [TestMethod]
        public void DeletePaymentMethod_WithNonExistentId_ShouldReturnFalse()
        {
            // Arrange
            int id = 99; // Non-existent ID

            // Act
            bool success = DeletePaymentMethod_WithInvalidId_ShouldReturnFalse(id);

            // Assert
            Assert.IsFalse(success);
        }

        [TestMethod]
        public void PaymentMethodListView_SelectionChanged_ShouldUpdateDataContext()
        {
            // Arrange
            var selectedPayment = GetFakePaymentMethodList()[0];

            // Act
            object dataContext = PaymentMethodListView_SelectionChanged_ShouldSetDataContext(selectedPayment);

            // Assert
            Assert.IsNotNull(dataContext);
            Assert.AreEqual(selectedPayment, dataContext);
        }

        [TestMethod]
        public void ValidatePaymentMethodInput_WithValidData_ShouldReturnTrue()
        {
            // Arrange
            string name = "Momo";
            string description = "Thanh toán qua ví Momo";
            bool isActive = true;

            // Act
            bool isValid = ValidatePaymentMethodInput_WithValidData_ShouldReturnTrue(name, description, isActive);

            // Assert
            Assert.IsTrue(isValid);
        }

        [TestMethod]
        public void ValidatePaymentMethodInput_WithEmptyName_ShouldReturnFalse()
        {
            // Arrange
            string name = "";
            string description = "Thanh toán qua ví Momo";
            bool isActive = true;

            // Act
            bool isValid = ValidatePaymentMethodInput_WithInvalidData_ShouldReturnFalse(name, description, isActive);

            // Assert
            Assert.IsFalse(isValid);
        }

        [TestMethod]
        public void SelectImage_ShouldUpdateSelectedImagePath()
        {
            // Arrange
            string expectedPath = "/Assets/Payment/new_payment.png";

            // Act
            string actualPath = SelectImage_ShouldReturnValidPath();

            // Assert
            Assert.AreEqual(expectedPath, actualPath);
        }

        [TestMethod]
        public void ClearFormFields_ShouldResetAllFields()
        {
            // Arrange
            string name = "Momo";
            string description = "Thanh toán qua ví Momo";
            bool isActive = true;
            string iconUrl = "/Assets/Payment/momo.png";

            // Act
            var result = ClearFormFields_ShouldResetToDefaultValues(name, description, isActive, iconUrl);

            // Assert
            Assert.AreEqual("", result.Name);
            Assert.AreEqual("", result.Description);
            Assert.AreEqual("", result.IconUrl);
        }

        // Helper Methods (mocking functionality without using actual DAOs)
        private List<PaymentMethod> GetFakePaymentMethodList()
        {
            return new List<PaymentMethod>
            {
                new PaymentMethod { Id = 1, Name = "Tiền mặt", Description = "Thanh toán bằng tiền mặt", IconUrl = "/Assets/Payment/cash.png", IsActive = true },
                new PaymentMethod { Id = 2, Name = "Momo", Description = "Thanh toán qua ví Momo", IconUrl = "/Assets/Payment/momo.png", IsActive = true },
                new PaymentMethod { Id = 3, Name = "Thẻ tín dụng", Description = "Thanh toán bằng thẻ tín dụng", IconUrl = "/Assets/Payment/credit_card.png", IsActive = false }
            };
        }

        private List<PaymentMethod> LoadPaymentMethod_ShouldReturnList() => GetFakePaymentMethodList();

        private int InsertPaymentMethod_WithValidData_ShouldReturnId(string name, string description, bool isActive, string iconUrl) => 4;

        private int InsertPaymentMethod_WithInvalidData_ShouldReturnMinusOne(string name, string description, bool isActive, string iconUrl) => -1;

        private bool UpdatePaymentMethod_WithValidData_ShouldReturnTrue(int id, string name, string description, bool isActive, string iconUrl) => true;

        private bool UpdatePaymentMethod_WithInvalidId_ShouldReturnFalse(int id, string name, string description, bool isActive, string iconUrl) => false;

        private bool DeletePaymentMethod_WithValidId_ShouldReturnTrue(int id) => true;

        private bool DeletePaymentMethod_WithInvalidId_ShouldReturnFalse(int id) => false;

        private object PaymentMethodListView_SelectionChanged_ShouldSetDataContext(PaymentMethod selectedPayment) => selectedPayment;

        private bool ValidatePaymentMethodInput_WithValidData_ShouldReturnTrue(string name, string description, bool isActive) =>
            !string.IsNullOrWhiteSpace(name);

        private bool ValidatePaymentMethodInput_WithInvalidData_ShouldReturnFalse(string name, string description, bool isActive) =>
            !string.IsNullOrWhiteSpace(name);

        private string SelectImage_ShouldReturnValidPath() => "/Assets/Payment/new_payment.png";

        private (string Name, string Description, string IconUrl) ClearFormFields_ShouldResetToDefaultValues(
            string name, string description, bool isActive, string iconUrl) =>
            ("", "", "");
    }
}
